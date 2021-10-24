namespace Coding4fun.DataTools.Common
{
    internal class ColumnDescription
    {
        public ColumnDescription(string sqlColumnName, string sqlType, string valueBody)
        {
            SqlColumnName = sqlColumnName;
            SqlType = sqlType;
            ValueBody = valueBody;
        }

        public string SqlColumnName { get; }
        public string SqlType { get; }
        public string ValueBody { get; }

        /// <inheritdoc />
        public override string ToString() => $"{nameof(SqlColumnName)}={SqlColumnName}," +
                                             $"{nameof(SqlType)}={SqlType}," +
                                             $"{nameof(ValueBody)}={ValueBody}";
    }
}