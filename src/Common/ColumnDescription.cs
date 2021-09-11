namespace Coding4fun.DataTableGenerator.Common
{
    internal class ColumnDescription
    {
        public ColumnDescription(string sqlColumnName, string sqlType, string? valueBody, string varName)
        {
            SqlColumnName = sqlColumnName;
            SqlType = sqlType;
            ValueBody = valueBody;
            VarName = varName;
        }

        /// <summary>
        ///     Column name for SQL table.
        /// </summary>
        public string SqlColumnName { get; }

        /// <summary>
        ///     Column type for SQL table.
        /// </summary>
        public string SqlType { get; }

        /// <summary>
        ///     C# value getter from expression.
        /// </summary>
        public string? ValueBody { get; }

        /// <summary>
        ///     C# item name.
        /// </summary>
        public string VarName { get; set; }

        /// <inheritdoc />
        public override string ToString() => $"{nameof(SqlColumnName)}={SqlColumnName}," +
                                             $"{nameof(SqlType)}={SqlType}," +
                                             $"{nameof(VarName)}={VarName}," +
                                             $"{nameof(ValueBody)}={ValueBody}";
    }
}