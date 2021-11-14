using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Example
{
    internal class TableBuilder<TItem>
    {
        internal TableBuilder<TItem> AddColumn(
            Expression<Func<TItem, object>> valueGetter,
            string sqlType = null,
            string columnName = null) => this;

        internal TableBuilder<TItem> AddSubTable<TSubItem>(
            Expression<Func<TItem, IEnumerable<TSubItem>>> enumerableGetter,
            Action<SubTableBuilder<TSubItem, TItem>> subTableConsumer) => this;
    }

    internal class SubTableBuilder<TItem, TParentItem>
    {
        internal SubTableBuilder<TItem, TParentItem> AddSubTable<TSubItem>(
            Expression<Func<TItem, IEnumerable<TSubItem>>> enumerableGetter,
            Action<SubTableBuilder<TSubItem, TItem>> subTableConsumer) => this;

        internal SubTableBuilder<TItem, TParentItem> AddColumn(
            Expression<Func<TItem, object>> valueGetter,
            string sqlType = null,
            string customSqlColumnName = null) => this;
    }

    internal class Person
    {
        internal Contact Contact { get; set; }
    }

    internal class Contact
    {
        internal string Phone { get; set; }
        internal string Email { get; set; }
        internal List<Contact> AnotherContacts { get; set; }
    }

    class Program
    {
        static void Main()
        {
            new TableBuilder<Person>();
        }
    }
}