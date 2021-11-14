using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Coding4fun.DataTools.Analyzers
{
    [DebuggerDisplay("{ToString()}")]
    internal class ColumnDescription
    {
        public ColumnDescription(string valueBody, string? sqlColumnName = null, string? sqlType = null)
        {
            SqlColumnName = sqlColumnName;
            SqlType = sqlType;
            ValueBody = valueBody;
        }

        public string? SqlColumnName { get; }
        public string? SqlType { get; }
        public string ValueBody { get; }
        public string? SharpType { get; set; }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        public override string ToString() => $"{nameof(SqlColumnName)}={SqlColumnName}," +
                                             $"{nameof(SqlType)}={SqlType}," +
                                             $"{nameof(ValueBody)}={ValueBody}";
    }
}