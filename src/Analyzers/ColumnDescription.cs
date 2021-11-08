namespace Coding4fun.DataTools.Analyzers
{
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
        public override string ToString() => $"{nameof(SqlColumnName)}={SqlColumnName}," +
                                             $"{nameof(SqlType)}={SqlType}," +
                                             $"{nameof(ValueBody)}={ValueBody}";
    }
}