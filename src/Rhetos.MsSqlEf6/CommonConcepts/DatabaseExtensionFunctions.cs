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

using Rhetos.Persistence;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

#pragma warning disable CA1309 // Use ordinal string comparison

namespace Rhetos.Dom.DefaultConcepts
{
    /// <summary>
    /// This class contains a set of SQL-compatible functions that can be evaluated in both LINQ and SQL.
    /// </summary>
    public static class DatabaseExtensionFunctions
    {
        /// <summary>
        /// If b is null, SQL query will use IS NULL instead of the equality operator.
        /// </summary>
        [DbFunction(EntityFrameworkMapping.ConceptualModelNamespace, "StringEqualsCaseInsensitive")]
        public static bool EqualsCaseInsensitive(this string a, string b)
        {
            if (b == null)
                return a == null;
            else
                return a != null && a.Equals(b, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// If b is null, SQL query will use IS NOT NULL instead of the inequality operator.
        /// </summary>
        [DbFunction(EntityFrameworkMapping.ConceptualModelNamespace, "StringNotEqualsCaseInsensitive")]
        public static bool NotEqualsCaseInsensitive(this string a, string b)
        {
            if (b == null)
                return a != null;
            else
                return a == null || !a.Equals(b, StringComparison.InvariantCultureIgnoreCase);
        }

        [DbFunction(EntityFrameworkMapping.ConceptualModelNamespace, "StringIsLessThen")]
        public static bool IsLessThen(this string a, string b)
        {
            return a != null && b != null && String.Compare(a, b, StringComparison.InvariantCultureIgnoreCase) < 0;
        }

        [DbFunction(EntityFrameworkMapping.ConceptualModelNamespace, "StringIsLessThenOrEqual")]
        public static bool IsLessThenOrEqual(this string a, string b)
        {
            return a != null && b != null && String.Compare(a, b, StringComparison.InvariantCultureIgnoreCase) <= 0;
        }

        [DbFunction(EntityFrameworkMapping.ConceptualModelNamespace, "StringIsGreaterThen")]
        public static bool IsGreaterThen(this string a, string b)
        {
            return a != null && b != null && String.Compare(a, b, StringComparison.InvariantCultureIgnoreCase) > 0;
        }

        [DbFunction(EntityFrameworkMapping.ConceptualModelNamespace, "StringIsGreaterThenOrEqual")]
        public static bool IsGreaterThenOrEqual(this string a, string b)
        {
            return a != null && b != null && String.Compare(a, b, StringComparison.InvariantCultureIgnoreCase) >= 0;
        }

        [DbFunction(EntityFrameworkMapping.ConceptualModelNamespace, "IntStartsWith")]
        public static bool StartsWith(this int? a, string b)
        {
            return a != null && b != null && a.ToString().StartsWith(b);
        }

        [DbFunction(EntityFrameworkMapping.ConceptualModelNamespace, "StringStartsWithCaseInsensitive")]
        public static bool StartsWithCaseInsensitive(this string a, string b)
        {
            return a != null && b != null && a.StartsWith(b, StringComparison.InvariantCultureIgnoreCase);
        }

        [DbFunction(EntityFrameworkMapping.ConceptualModelNamespace, "StringEndsWithCaseInsensitive")]
        public static bool EndsWithCaseInsensitive(this string a, string b)
        {
            return a != null && b != null && a.EndsWith(b, StringComparison.InvariantCultureIgnoreCase);
        }

        [DbFunction(EntityFrameworkMapping.ConceptualModelNamespace, "StringContainsCaseInsensitive")]
        public static bool ContainsCaseInsensitive(this string a, string b)
        {
            return a != null && b != null && a.Contains(b, StringComparison.InvariantCultureIgnoreCase);
        }

        [DbFunction(EntityFrameworkMapping.ConceptualModelNamespace, "StringLike")]
        public static bool Like(this string text, string pattern)
        {
            if (text == null || pattern == null)
                return false;

            pattern = Regex.Escape(pattern);
            pattern = pattern.Replace("%", ".*");
            pattern = pattern.Replace("_", ".");
            pattern = "^" + pattern + "$";

            var regex = new Regex(pattern, RegexOptions.IgnoreCase);
            return regex.IsMatch(text);
        }

        [DbFunction(EntityFrameworkMapping.ConceptualModelNamespace, "IntCastToString")]
        public static string CastToString(this int? a)
        {
            if (a == null)
                return null;

            return a.ToString();
        }

        [DbFunction(EntityFrameworkMapping.ConceptualModelNamespace, "GuidIsGreaterThan")]
        public static bool GuidIsGreaterThan(this Guid g1, Guid g2)
        {
            string a = g1.ToString();
            string b = g2.ToString();
            return a != null && b != null && String.Compare(a, b, StringComparison.InvariantCultureIgnoreCase) > 0;
        }

        [DbFunction(EntityFrameworkMapping.ConceptualModelNamespace, "GuidIsGreaterThanOrEqual")]
        public static bool GuidIsGreaterThanOrEqual(this Guid g1, Guid g2)
        {
            string a = g1.ToString();
            string b = g2.ToString();
            return a != null && b != null && String.Compare(a, b, StringComparison.InvariantCultureIgnoreCase) >= 0;
        }

        [DbFunction(EntityFrameworkMapping.ConceptualModelNamespace, "GuidIsLessThan")]
        public static bool GuidIsLessThan(this Guid g1, Guid g2)
        {
            string a = g1.ToString();
            string b = g2.ToString();
            return a != null && b != null && String.Compare(a, b, StringComparison.InvariantCultureIgnoreCase) < 0;
        }

        [DbFunction(EntityFrameworkMapping.ConceptualModelNamespace, "GuidIsLessThanOrEqual")]
        public static bool GuidIsLessThanOrEqual(this Guid g1, Guid g2)
        {
            string a = g1.ToString();
            string b = g2.ToString();
            return a != null && b != null && String.Compare(a, b, StringComparison.InvariantCultureIgnoreCase) <= 0;
        }

        public const string InterceptFullTextSearchFunction = "InterceptFullTextSearch";

#pragma warning disable CA1801 // Review unused parameters

        /// <param name="tableName">Table that contains the full-text search indexed columns. It is usually the entity's table or a table that references the entity's table.</param>
        /// <param name="searchColumns">Full-text search indexed columns. See the columns list parameter on CONTAINSTABLE function for Microsoft SQL server database.</param>
        [DbFunction(EntityFrameworkMapping.StorageModelNamespace, InterceptFullTextSearchFunction)]
        public static bool FullTextSearch(Guid itemId, string pattern, string tableName, string searchColumns)
        {
            throw new ClientException("Full-text search cannot be executed on loaded data. Use this function in a LINQ query to execute FTS on database.");
        }

        /// <param name="tableName">Table that contains the full-text search indexed columns. It is usually the entity's table or a table that references the entity's table.</param>
        /// <param name="searchColumns">Full-text search indexed columns. See the columns list parameter on CONTAINSTABLE function for Microsoft SQL server database.</param>
        [DbFunction(EntityFrameworkMapping.StorageModelNamespace, InterceptFullTextSearchFunction)]
        public static bool FullTextSearch(Guid itemId, string pattern, string tableName, string searchColumns, int? rankTop)
        {
            throw new ClientException("Full-text search cannot be executed on loaded data. Use this function in a LINQ query to execute FTS on database.");
        }

        /// <param name="tableName">Table that contains the full-text search indexed columns. It is usually the entity's table or a table that references the entity's table.</param>
        /// <param name="searchColumns">Full-text search indexed columns. See the columns list parameter on CONTAINSTABLE function for Microsoft SQL server database.</param>
        [DbFunction(EntityFrameworkMapping.StorageModelNamespace, InterceptFullTextSearchFunction)]
        public static bool FullTextSearch(int itemId, string pattern, string tableName, string searchColumns)
        {
            throw new ClientException("Full-text search cannot be executed on loaded data. Use this function in a LINQ query to execute FTS on database.");
        }

        /// <param name="tableName">Table that contains the full-text search indexed columns. It is usually the entity's table or a table that references the entity's table.</param>
        /// <param name="searchColumns">Full-text search indexed columns. See the columns list parameter on CONTAINSTABLE function for Microsoft SQL server database.</param>
        [DbFunction(EntityFrameworkMapping.StorageModelNamespace, InterceptFullTextSearchFunction)]
        public static bool FullTextSearch(int itemId, string pattern, string tableName, string searchColumns, int? rankTop)
        {
            throw new ClientException("Full-text search cannot be executed on loaded data. Use this function in a LINQ query to execute FTS on database.");
        }

#pragma warning restore CA1801 // Review unused parameters
    }
}

#pragma warning restore CA1309 // Use ordinal string comparison
