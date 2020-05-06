﻿/*
    Copyright (C) 2014 Omega software d.o.o.

    This file is part of Rhetos.

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as
    published by the Free Software Foundation, either version 3 of the
    License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using Autofac;
using Rhetos.Extensibility;
using Rhetos.Logging;
using Rhetos.Security;
using Rhetos.Utilities;
using System;
using System.Diagnostics;
using System.Threading;

namespace Rhetos.Configuration.Autofac
{
    /// <summary>
    /// It encapsulates a Dependency Injection container (see <see cref="Host.CreateRhetosContainer"/>)
    /// for creating the lifetime-scope child containers with <see cref="CreateTransactionScope(Action{ContainerBuilder})"/>.
    /// Use the child containers to isolate units of work into separate atomic transactions.
    /// 
    /// RhetosProcessContainer is thread-safe: the main RhetosProcessContainer instance can be reused between threads
    /// to reduce the initialization time, such as plugin discovery and Entity Framework startup.
    /// Each thread should use <see cref="CreateTransactionScope(Action{ContainerBuilder})"/> to create its own lifetime-scope child container.
    /// 
    /// RhetosProcessContainer overrides the main application's DI components to use <see cref="ProcessUserInfo"/>
    /// and <see cref="ConsoleLogProvider"/> by default.
    /// It also registers assembly resolver for runtime assemblies.
    /// </summary>
    public class RhetosProcessContainer : IDisposable
    {
        private readonly Lazy<Host> _host;
        private readonly Lazy<IConfiguration> _configuration;
        private readonly Lazy<IContainer> _rhetosIocContainer;
        private ResolveEventHandler _assemblyResolveEventHandler = null;

        public IConfiguration Configuration => _configuration.Value;

        /// <param name="applicationFolder">
        /// Folder where the Rhetos configuration file is located (see <see cref="RhetosAppEnvironment.ConfigurationFileName"/>),
        /// or any subfolder.
        /// If not specified, the current application's base directory is used by default.
        /// </param>
        /// <param name="logProvider">
        /// If not specified, ConsoleLogProvider is used by default.
        /// </param>
        /// <param name="registerCustomComponents">
        /// Register custom components that may override system and plugins services.
        /// This is commonly used by utilities and tests that need to override host application's components or register additional plugins.
        /// </param>
        public RhetosProcessContainer(string applicationFolder = null, ILogProvider logProvider = null,
            Action<IConfigurationBuilder> addCustomConfiguration = null, Action<ContainerBuilder> registerCustomComponents = null)
        {
            logProvider = logProvider ?? new ConsoleLogProvider();
            if (applicationFolder == null)
                applicationFolder = AppDomain.CurrentDomain.BaseDirectory;

            _host = new Lazy<Host>(() => Host.Find(applicationFolder, logProvider), LazyThreadSafetyMode.ExecutionAndPublication);
            _configuration = new Lazy<IConfiguration>(() => _host.Value.RhetosRuntime.BuildConfiguration(logProvider, _host.Value.ConfigurationFolder, addCustomConfiguration), LazyThreadSafetyMode.ExecutionAndPublication);
            _rhetosIocContainer = new Lazy<IContainer>(() => BuildRhetosProcessContainer(logProvider, registerCustomComponents), LazyThreadSafetyMode.ExecutionAndPublication);
        }

        private IContainer BuildRhetosProcessContainer(ILogProvider logProvider, Action<ContainerBuilder> registerCustomComponents)
        {
            // The values for rhetosRuntime and configuration are resolved before the call to Stopwatch.StartNew
            // so that the performance logging only takes into account the time needed to build the IOC container
            var sw = Stopwatch.StartNew();

            var runtimeAssemblies = AssemblyResolver.GetRuntimeAssemblies(_configuration.Value);
            _assemblyResolveEventHandler = AssemblyResolver.GetResolveEventHandler(runtimeAssemblies, logProvider);
            AppDomain.CurrentDomain.AssemblyResolve += _assemblyResolveEventHandler;

            var iocContainer = _host.Value.RhetosRuntime.BuildContainer(logProvider, _configuration.Value, builder =>
            {
                // Override runtime IUserInfo plugins. This container is intended to be used in unit tests or
                // in a process that is executed directly by user, usually by developer or administrator.
                builder.RegisterType<ProcessUserInfo>().As<IUserInfo>();
                builder.RegisterType<ConsoleLogProvider>().As<ILogProvider>();
                registerCustomComponents?.Invoke(builder);
            });
            logProvider.GetLogger("Performance").Write(sw, $"{nameof(RhetosTransactionScopeContainer)}: Built IoC container");
            return iocContainer;
        }

        /// <summary>
        /// This method creates a thread-safe lifetime scope DI container to isolate unit of work in a separate database transaction.
        /// To commit changes to database, call <see cref="RhetosTransactionScopeContainer.CommitChanges"/> at the end of the 'using' block.
        /// </summary>
        /// <param name="registerCustomComponents">
        /// Register custom components that may override system and plugins services.
        /// This is commonly used by utilities and tests that need to override host application's components or register additional plugins.
        /// </param>
        public RhetosTransactionScopeContainer CreateTransactionScope(Action<ContainerBuilder> registerCustomComponents = null)
        {
            return new RhetosTransactionScopeContainer(_rhetosIocContainer.Value, registerCustomComponents);
        }

        #region Static helper for singleton RhetosProcessContainer. Useful optimization for LINQPad scripts that reuse the external static instance after recompiling the script.

        private static RhetosProcessContainer _singleContainer = null;
        private static string _singleContainerApplicationFolder = null;
        private static object _singleContainerLock = new object();

        /// <summary>
        /// This method creates a thread-safe lifetime scope DI container to isolate unit of work in a separate database transaction.
        /// To commit changes to database, call <see cref="RhetosTransactionScopeContainer.CommitChanges"/> at the end of the 'using' block.
        /// 
        /// In most cases it is preferred to use a <see cref="RhetosProcessContainer"/> instance instead of this static method, for better control over the DI container.
        /// The static method is useful in some special cases, for example to optimize LINQPad scripts that can reuse the external static instance
        /// after recompiling the script.
        /// </summary>
        /// <param name="applicationFolder">
        /// Folder where the Rhetos configuration file is located (see <see cref="RhetosAppEnvironment.ConfigurationFileName"/>),
        /// or any subfolder.
        /// If not specified, the current application's base directory is used by default.
        /// </param>
        /// <param name="registerCustomComponents">
        /// Register custom components that may override system and plugins services.
        /// This is commonly used by utilities and tests that need to override host application's components or register additional plugins.
        /// </param>
        public static RhetosTransactionScopeContainer CreateTransactionScope(string applicationFolder = null, Action<ContainerBuilder> registerCustomComponents = null)
        {
            if (_singleContainer == null)
                lock (_singleContainerLock)
                    if (_singleContainer == null)
                    {
                        _singleContainerApplicationFolder = applicationFolder;
                        _singleContainer = new RhetosProcessContainer(applicationFolder);
                    }

            if (_singleContainerApplicationFolder != applicationFolder)
                throw new FrameworkException($"Static {nameof(RhetosProcessContainer)}.{nameof(CreateTransactionScope)} cannot be used for different" +
                    $" application contexts: Provided folder 1: '{_singleContainerApplicationFolder}', folder 2: '{applicationFolder}'." +
                    $" Use a {nameof(RhetosProcessContainer)} instances instead.");

            return _singleContainer.CreateTransactionScope(registerCustomComponents);
        }

        #endregion

        #region Standard IDisposable pattern

        private bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                if (_rhetosIocContainer.IsValueCreated)
                    _rhetosIocContainer.Value.Dispose();
                if (_assemblyResolveEventHandler != null)
                    AppDomain.CurrentDomain.AssemblyResolve -= _assemblyResolveEventHandler;
            }

            disposed = true;
        }

        #endregion
    }
}
