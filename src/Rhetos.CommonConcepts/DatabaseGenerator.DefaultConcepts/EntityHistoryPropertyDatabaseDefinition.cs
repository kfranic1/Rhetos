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

using Rhetos.Compiler;
using Rhetos.Dsl;
using Rhetos.Dsl.DefaultConcepts;
using Rhetos.Extensibility;
using Rhetos.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace Rhetos.DatabaseGenerator.DefaultConcepts
{
    [Export(typeof(IConceptDatabaseDefinition))]
    [ExportMetadata(MefProvider.Implements, typeof(EntityHistoryPropertyInfo))]
    public class EntityHistoryPropertyDatabaseDefinition : IConceptDatabaseDefinitionExtension
    {
        private readonly ISqlUtility _sqlUtility;

        public EntityHistoryPropertyDatabaseDefinition(ISqlUtility sqlUtility)
        {
            _sqlUtility = sqlUtility;
        }

        public string CreateDatabaseStructure(IConceptInfo conceptInfo)
        {
            return null;
        }

        public string RemoveDatabaseStructure(IConceptInfo conceptInfo)
        {
            return null;
        }

        public void ExtendDatabaseStructure(IConceptInfo conceptInfo, ICodeBuilder codeBuilder, out IEnumerable<Tuple<IConceptInfo, IConceptInfo>> createdDependencies)
        {
            var info = (EntityHistoryPropertyInfo)conceptInfo;
            createdDependencies = null;

            var columnName = GetColumnName(info.Property);

            codeBuilder.InsertCode(string.Format(",\r\n        {0} = history.{0}", columnName),
                EntityHistoryMacro.SelectHistoryPropertiesTag, info.Dependency_EntityHistory);

            codeBuilder.InsertCode(string.Format(",\r\n        {0} = entity.{0}", columnName),
                EntityHistoryMacro.SelectEntityPropertiesTag, info.Dependency_EntityHistory);
        }

        private string GetColumnName(PropertyInfo property)
        {
            if (property is ReferencePropertyInfo)
                return _sqlUtility.Identifier(property.Name + "ID");
            return _sqlUtility.Identifier(property.Name);
        }
    }
}
