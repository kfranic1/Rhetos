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

using Newtonsoft.Json;
using Rhetos.Logging;
using Rhetos.Utilities;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Rhetos.DatabaseGenerator
{
    public class DatabaseModelFile
    {
        private const string DatabaseModelFileName = "DatabaseModel.json";

        private readonly ILogger _performanceLogger;
        private readonly string _databaseModelFilePath;

        public DatabaseModelFile(
            ILogProvider logProvider,
            IRhetosEnvironment rhetosEnvironment)
        {
            _performanceLogger = logProvider.GetLogger("Performance");
            _databaseModelFilePath = Path.Combine(rhetosEnvironment.AssetsFolder, DatabaseModelFileName);
        }

        public void Save(DatabaseModel databaseModel)
        {
            var stopwatch = Stopwatch.StartNew();

            string serializedModel = JsonConvert.SerializeObject(databaseModel, JsonSerializerSettings);
            _performanceLogger.Write(stopwatch, $"{nameof(DatabaseModelFile)}.{nameof(Save)}: Serialize.");

            File.WriteAllText(_databaseModelFilePath, serializedModel, Encoding.UTF8);
            _performanceLogger.Write(stopwatch, $"{nameof(DatabaseModelFile)}.{nameof(Save)}: Write.");
        }

        public DatabaseModel Load()
        {
            var stopwatch = Stopwatch.StartNew();

            string serializedModel;
            try
            {
                serializedModel = File.ReadAllText(_databaseModelFilePath, Encoding.UTF8);
            }
            catch (FileNotFoundException ex)
            {
                throw new FrameworkException("Cannot update database because the database model was not generated." +
                    " Please check that the build has completed successfully before updating the database.", ex);
            }
            var databaseModel = JsonConvert.DeserializeObject<DatabaseModel>(serializedModel, JsonSerializerSettings);
            _performanceLogger.Write(stopwatch, $"{nameof(DatabaseModelFile)}.{nameof(Load)}.");

            return databaseModel;
        }

        JsonSerializerSettings JsonSerializerSettings => new JsonSerializerSettings
        {
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            Formatting = Formatting.Indented,
        };
    }
}
