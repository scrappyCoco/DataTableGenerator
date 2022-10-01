using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Coding4fun.DataTools.Analyzers
{
    [PublicAPI]
    [ExcludeFromCodeCoverage]
    public class TableBuilder<TItem>
    {
        public const string Name = nameof(TableBuilder<TItem>);
        public TableBuilder(string templateGroup = Templates.DataTable)
        {
        }

        public TableBuilder<TItem> AddColumn(
            Func<TItem, object> valueGetter,
            string? sqlType = null,
            string? columnName = null) => this;

        public TableBuilder<TItem> AddPreExecutionAction(Action<TItem> itemAction) => this;

        public TableBuilder<TItem> SetName(string sqlTableName) => this;

        public TableBuilder<TItem> AddSubTable<TSubItem>(
            Func<TItem, IEnumerable<TSubItem>> enumerableGetter,
            Action<SubTableBuilder<TSubItem, TItem>> subTableConsumer) => this;
    }
}