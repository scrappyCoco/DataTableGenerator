using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Coding4fun.PainlessUtils;

namespace Coding4fun.DataTableGenerator.Common
{
    public class DataTableBuilder<T>
    {
        private readonly TableDescription _tableDescription;
        
        public DataTableBuilder(string? tableName = null)
        {
            if (string.IsNullOrWhiteSpace(tableName)) throw new ArgumentException("Invalid table name");
            string className = typeof(T).Name;
            _tableDescription = new TableDescription(tableName, className);
        }

        public DataTableBuilder<T> AddColumn<TItem>(
            string sqlColumnName,
            string sqlType,
            Expression<Func<T, TItem>> valueGetter)
        {
            string valueBody = valueGetter.Body.ToString();
            _tableDescription.Columns.Add(new ColumnDescription(sqlColumnName, sqlType, valueBody,
                valueGetter.Parameters[0].Name));
            return this;
        }

        public DataTableBuilder<T> AddSubTable<TSub>(
            string subTableName,
            Expression<Func<T, IEnumerable<TSub>>> enumerableGetter,
            Action<DataTableBuilder<TSub>> subTableConsumer)
        {
            var subTableBuilder = new DataTableBuilder<TSub>(subTableName);
            _tableDescription.SubTables.Add(subTableBuilder._tableDescription);
            subTableBuilder._tableDescription.EnumerableName = enumerableGetter.Body.ToString();
            subTableConsumer.Invoke(subTableBuilder);
            return this;
        }

        public DataTableBuilder<T> AddBasicSubTable<TItem>(
            string subTableName,
            string sqlColumnName,
            string sqlType,
            Expression<Func<T, IEnumerable<TItem>>> enumerableGetter)
        {
            var subTableDescription = new TableDescription(subTableName, typeof(TItem).Name);
            _tableDescription.SubTables.Add(subTableDescription);
            subTableDescription.Columns.Add(new ColumnDescription(sqlColumnName, sqlType, "item", "item"));
            subTableDescription.EnumerableName = enumerableGetter.Body.ToString();
            return this;
        }
    }
}