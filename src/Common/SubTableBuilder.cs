using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace Coding4fun.DataTools.Common
{
    [PublicAPI]
    public class SubTableBuilder<TItem, TParentItem>
    {
        public SubTableBuilder<TItem, TParentItem> AddColumn(
            Expression<Func<TItem, object>> valueGetter,
            string? sqlType = null,
            string? customSqlColumnName = null) => this;
        
        public SubTableBuilder<TItem, TParentItem> AddPreExecutionAction(Action<TItem, TParentItem> itemAction) => this;

        public SubTableBuilder<TItem, TParentItem> SetName(string sqlTableName) => this;

        public SubTableBuilder<TItem, TParentItem> AddSubTable<TSubItem>(
            Expression<Func<TItem, IEnumerable<TSubItem>>> enumerableGetter,
            Action<SubTableBuilder<TSubItem, TItem>> subTableConsumer) => this;

        public SubTableBuilder<TItem, TParentItem> AddBasicSubTable<TSubItem>(
            string subTableName,
            string sqlColumnName,
            string sqlType,
            Expression<Func<TItem, IEnumerable<TSubItem>>> enumerableGetter) => this;
    }
}