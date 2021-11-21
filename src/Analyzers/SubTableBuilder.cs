using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Coding4fun.DataTools.Analyzers
{
    [PublicAPI]
    [ExcludeFromCodeCoverage]
    public class SubTableBuilder<TItem, TParentItem>
    {
        public SubTableBuilder<TItem, TParentItem> AddColumn(
            Func<TItem, object> valueGetter,
            string? sqlType = null,
            string? customSqlColumnName = null) => this;
        
        public SubTableBuilder<TItem, TParentItem> AddPreExecutionAction(Action<TItem, TParentItem> itemAction) => this;

        public SubTableBuilder<TItem, TParentItem> SetName(string sqlTableName) => this;

        public SubTableBuilder<TItem, TParentItem> AddSubTable<TSubItem>(
            Func<TItem, IEnumerable<TSubItem>> enumerableGetter,
            Action<SubTableBuilder<TSubItem, TItem>> subTableConsumer) => this;
    }
}