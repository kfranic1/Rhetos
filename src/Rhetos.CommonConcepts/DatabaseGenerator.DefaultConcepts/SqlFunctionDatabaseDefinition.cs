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

using System;
using System.ComponentModel.Composition;
using Rhetos.Utilities;
using Rhetos.Extensibility;
using Rhetos.Dsl.DefaultConcepts;
using Rhetos.Dsl;
using System.Globalization;
using Rhetos.Compiler;

namespace Rhetos.DatabaseGenerator.DefaultConcepts
{
    [Export(typeof(IConceptDatabaseDefinition))]
    [ExportMetadata(MefProvider.Implements, typeof(SqlFunctionInfo))]
    public class SqlFunctionDatabaseDefinition : IConceptDatabaseDefinition
    {
        protected ISqlResources Sql { get; private set; }

        protected ISqlUtility SqlUtility { get; private set; }

        public SqlFunctionDatabaseDefinition(ISqlResources sqlResources, ISqlUtility sqlUtility)
        {
            this.Sql = sqlResources;
            this.SqlUtility = sqlUtility;
        }

        public string CreateDatabaseStructure(IConceptInfo conceptInfo)
        {
            var info = (SqlFunctionInfo)conceptInfo;

            return Sql.Format("SqlFunctionDatabaseDefinition_Create",
                    SqlUtility.Identifier(info.Module.Name),
                    SqlUtility.Identifier(info.Name),
                    info.Arguments,
                    info.Source,
                    Rhetos.Utilities.SqlUtility.ScriptSplitterTag);
        }

        public string RemoveDatabaseStructure(IConceptInfo conceptInfo)
        {
            var info = (SqlFunctionInfo)conceptInfo;

            return Sql.Format("SqlFunctionDatabaseDefinition_Remove", SqlUtility.Identifier(info.Module.Name), SqlUtility.Identifier(info.Name));
        }
    }
}
