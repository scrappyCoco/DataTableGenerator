using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Coding4fun.DataTools.Common.StringUtil;
using JetBrains.Annotations;

namespace Coding4fun.DataTools.Common
{
    [PublicAPI]
    public class TableBuilder<TItem>
    {
        public const string Name = nameof(TableBuilder<TItem>);
        public TableBuilder(NamingConvention namingConvention = NamingConvention.ScreamingSnakeCase)
        {
        }

        public TableBuilder<TItem> AddColumn(
            Expression<Func<TItem, object>> valueGetter,
            string? sqlType = null,
            string? columnName = null) =>  this;

        public TableBuilder<TItem> AddPreExecutionAction(Action<TItem> itemAction) => this;

        public TableBuilder<TItem> SetName(string sqlTableName) => this;

        public TableBuilder<TItem> AddSubTable<TSubItem>(
            Expression<Func<TItem, IEnumerable<TSubItem>>> enumerableGetter,
            Action<SubTableBuilder<TSubItem, TItem>> subTableConsumer) => this;
    }
}