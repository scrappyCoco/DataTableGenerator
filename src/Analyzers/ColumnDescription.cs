using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Coding4fun.DataTools.Analyzers.Extension;

namespace Coding4fun.DataTools.Analyzers
{
    [DebuggerDisplay("{ToString()}")]
    public class ColumnDescription
    {
        internal readonly Dictionary<string, string> CustomAttributes = new (StringComparer.InvariantCultureIgnoreCase);

        public ColumnDescription(string valueBody, string? columnName = null, string? sqlType = null)
        {
            ColumnName = columnName;
            SqlType = sqlType;
            ValueBody = valueBody;
        }

        public string? ColumnName { get; se; }
        public string? SqlType { get; }
        public string ValueBody { get; }
        public string? SharpType { get; set; }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        public override string ToString() => $"{nameof(ColumnName)}={ColumnName}," +
                                             $"{nameof(SqlType)}={SqlType}," +
                                             $"{nameof(ValueBody)}={ValueBody}";
    }
}