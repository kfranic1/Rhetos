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

using Rhetos.Deployment;
using Rhetos.SqlResources;
using Rhetos.Utilities;
using System.Collections.Generic;

namespace Rhetos.MsSql.SqlResources
{
    public class CoreDbUpdateSqlResourcesPlugin : ISqlResourcesPlugin
    {
        private readonly string _databaseLanguage;

        public CoreDbUpdateSqlResourcesPlugin(DatabaseSettings databaseSettings)
        {
            _databaseLanguage = databaseSettings.DatabaseLanguage;
        }

        public IDictionary<string, string> GetResources()
        {
            if (!_databaseLanguage.StartsWith(MsSqlUtility.DatabaseLanguage))
                return null;

            var resources = ResourcesUtility.ReadEmbeddedResx("Rhetos.Core.DbUpdate.MsSql.resx", GetType(), true);

            resources.Add(
                DatabaseDeployment.CreateRhetosDatabaseResourceKey,
                ResourcesUtility.ReadEmbeddedTextFile("Rhetos.Core.CreateDatabase.MsSql.sql", GetType(), true));

            return resources;
        }
    }
}
