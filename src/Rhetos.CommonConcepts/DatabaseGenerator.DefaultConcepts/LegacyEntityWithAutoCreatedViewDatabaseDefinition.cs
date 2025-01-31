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
using System.ComponentModel.Composition;

namespace Rhetos.DatabaseGenerator.DefaultConcepts
{
    [Export(typeof(IConceptDatabaseDefinition))]
    [ExportMetadata(MefProvider.Implements, typeof(LegacyEntityWithAutoCreatedViewInfo))]
    public class LegacyEntityWithAutoCreatedViewDatabaseDefinition : IConceptDatabaseDefinition
    {
        public static readonly SqlTag<LegacyEntityWithAutoCreatedViewInfo> ViewSelectPartTag = "ViewSelectPart";
        public static readonly SqlTag<LegacyEntityWithAutoCreatedViewInfo> ViewFromPartTag = "ViewFromPart";
        public static readonly SqlTag<LegacyEntityWithAutoCreatedViewInfo> TriggerInsertPartTag = "TriggerInsertPart";
        public static readonly SqlTag<LegacyEntityWithAutoCreatedViewInfo> TriggerSelectForInsertPartTag = "TriggerSelectForInsertPartTag";
        public static readonly SqlTag<LegacyEntityWithAutoCreatedViewInfo> TriggerSelectForUpdatePartTag = "TriggerSelectForUpdatePartTag";
        public static readonly SqlTag<LegacyEntityWithAutoCreatedViewInfo> TriggerFromPartTag = "TriggerFromPart";

        private readonly ISqlResources _sqlResources;
        private readonly ISqlUtility _sqlUtility;

        public LegacyEntityWithAutoCreatedViewDatabaseDefinition(ISqlResources sqlResources, ISqlUtility sqlUtility)
        {
            _sqlResources = sqlResources;
            _sqlUtility = sqlUtility;
        }

        protected string InsertTriggerName(LegacyEntityWithAutoCreatedViewInfo info)
        {
            return _sqlUtility.Identifier(_sqlResources.Format("LegacyEntityWithAutoCreatedViewDatabaseDefinition_InsertTriggerName", info.Name));
        }

        protected string UpdateTriggerName(LegacyEntityWithAutoCreatedViewInfo info)
        {
            return _sqlUtility.Identifier(_sqlResources.Format("LegacyEntityWithAutoCreatedViewDatabaseDefinition_UpdateTriggerName", info.Name));
        }

        protected string DeleteTriggerName(LegacyEntityWithAutoCreatedViewInfo info)
        {
            return _sqlUtility.Identifier(_sqlResources.Format("LegacyEntityWithAutoCreatedViewDatabaseDefinition_DeleteTriggerName", info.Name));
        }

        public string CreateDatabaseStructure(IConceptInfo conceptInfo)
        {
            var info = (LegacyEntityWithAutoCreatedViewInfo) conceptInfo;

            return _sqlResources.Format("LegacyEntityWithAutoCreatedViewDatabaseDefinition_Create",
                _sqlUtility.Identifier(info.Module.Name),
                _sqlUtility.Identifier(info.Name),
                SqlUtility.ScriptSplitterTag,
                _sqlUtility.GetFullName(info.Table),
                InsertTriggerName(info),
                UpdateTriggerName(info),
                DeleteTriggerName(info),
                ViewSelectPartTag.Evaluate(info),
                ViewFromPartTag.Evaluate(info),
                TriggerInsertPartTag.Evaluate(info),
                TriggerSelectForInsertPartTag.Evaluate(info),
                TriggerSelectForUpdatePartTag.Evaluate(info),
                TriggerFromPartTag.Evaluate(info));
        }

        public string RemoveDatabaseStructure(IConceptInfo conceptInfo)
        {
            var info = (LegacyEntityWithAutoCreatedViewInfo) conceptInfo;

            return _sqlResources.Format("LegacyEntityWithAutoCreatedViewDatabaseDefinition_Remove",
                _sqlUtility.Identifier(info.Module.Name),
                _sqlUtility.Identifier(info.Name),
                InsertTriggerName(info),
                UpdateTriggerName(info),
                DeleteTriggerName(info));
        }
    }
}
