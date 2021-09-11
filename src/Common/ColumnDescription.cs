namespace Coding4fun.DataTableGenerator.Common
{
    internal class ColumnDescription
    {
        public ColumnDescription(string sqlColumnName, string sqlType, string valueBody, string varName)
        {
            SqlColumnName = sqlColumnName;
            SqlType = sqlType;
            ValueBody = valueBody;
            VarName = varName;
        }

        public string SqlColumnName { get; }
        public string SqlType { get; }
        public string ValueBody { get; }
        public string VarName { get; set; }

        /// <inheritdoc />
        public override string ToString() => $"{nameof(SqlColumnName)}={SqlColumnName}," +
                                             $"{nameof(SqlType)}={SqlType}," +
                                             $"{nameof(VarName)}={VarName}," +
                                             $"{nameof(ValueBody)}={ValueBody}";
    }
}