﻿using Rhetos.Logging;
using Rhetos.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Rhetos.Dsl
{
    public class ExternalTextReader : IExternalTextReader
    {
        private readonly FilesUtility _filesUtility;
        private readonly ISqlResources _sqlResources;
        private readonly DslSyntax _dslSyntax;

        public ExternalTextReader(FilesUtility filesUtility, ISqlResources sqlResources, DslSyntax dslSyntax)
        {
            _filesUtility = filesUtility;
            _sqlResources = sqlResources;
            _dslSyntax = dslSyntax;
        }

        public ValueOrError<string> Read(DslScript dslScript, string relativePathOrResourceName)
        {
            var candidateSqlResources = GetSqlResourceKeys(dslScript, relativePathOrResourceName);
            var candidateFiles = GetFilePaths(dslScript, relativePathOrResourceName);

            foreach (var resourceKey in candidateSqlResources)
            {
                string sqlScript = _sqlResources.TryGet(resourceKey);
                if (sqlScript != null)
                    return sqlScript;
            }

            foreach (var filePath in candidateFiles)
            {
                if (File.Exists(filePath))
                    return _filesUtility.ReadAllText(filePath);
            }

            string errorMessage;
            if (candidateFiles.Count == 1 && candidateSqlResources.Count == 0)
            {
                errorMessage = $"Cannot find the file referenced in DSL script. File does not exist: '{candidateFiles.First()}'";
            }
            else
            {
                var candidatesInfo = candidateSqlResources.Select(key => $"SQL resource key '{key}'")
                    .Concat(candidateFiles.Select(path => $"file '{path}'"))
                    .Reverse() // Application developers might better understand the error if it begins with the simplest examples and ends with the more specific ones.
                    .ToList();

                errorMessage = "Cannot find the file or resource referenced in DSL script. None of the following exists:"
                    + string.Join(", ", candidatesInfo.Select(candidate => Environment.NewLine + candidate));
            }
            return ValueOrError.CreateError(errorMessage);
        }

        private ICollection<string> GetSqlResourceKeys(DslScript dslScript, string relativePathOrResourceName)
        {
            if (!IsSqlScript(relativePathOrResourceName))
                return Array.Empty<string>();

            // Intentionally interpreting the dslScript Name as a path, although it is not the actual disk path.
            // This allows the behavior of referenced SQL resources similar to the referenced files:
            // 1. two scripts from the same package can share a same referenced file. 
            // 2. moving a script to a different folder requires modifying a reference or moving the files.
            // In a similar manner, the reference SQL resources key does not contain the script name,
            // so that two script might reference the same resource, but the key contains the package name
            // and the relative location of the script withing the package, so that two scripts in
            // different locations do not reuse the same resource key accidentally.

            string dslScriptLogicalLocation = Path.GetDirectoryName(dslScript.Name);
            string fullPath = Path.Combine(dslScriptLogicalLocation, relativePathOrResourceName);
            string resourceKey = fullPath.Replace(@"\", "/"); // Normalizing the path to avoid any differences in resource keys between platforms.
            return new[] { resourceKey };
        }

        private ICollection<string> GetFilePaths(DslScript dslScript, string relativePathOrResourceName)
        {
            string dslScriptFolder = Path.GetDirectoryName(dslScript.Path);
            string fullPath = Path.Combine(dslScriptFolder,
                relativePathOrResourceName
                    // Application developer can use either Windows or Linux/MacOs path separator. We will convert it to cross-platform path separator, so that the same plugin might work on multiple platforms.
                    .Replace('\\', Path.DirectorySeparatorChar)
                    .Replace('/', Path.DirectorySeparatorChar));

            var candidateFilePaths = new List<string>(1);

            if (IsSqlScript(relativePathOrResourceName))
            {
                var directory = Path.GetDirectoryName(fullPath);
                var fileName = Path.GetFileNameWithoutExtension(fullPath);
                string fileExtension = Path.GetExtension(fullPath);

                candidateFilePaths.Add(Path.Combine(directory, fileName + "." + _dslSyntax.DatabaseLanguage + fileExtension));
                candidateFilePaths.Add(Path.Combine(directory, fileName + " (" + _dslSyntax.DatabaseLanguage + ")" + fileExtension));
            }

            candidateFilePaths.Add(fullPath); // Adding the base path at the *end* of the list, to look for SQL dialect-specific files before the generic SQL file.

            return candidateFilePaths;
        }

        private static bool IsSqlScript(string relativePathOrResourceName)
        {
            return Path.GetExtension(relativePathOrResourceName).Equals(".sql", StringComparison.OrdinalIgnoreCase);
        }
    }
}
