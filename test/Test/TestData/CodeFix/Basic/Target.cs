using System;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq;
using System.Linq.Expressions;
using System.Linq.Expressions;

namespace Example
{
    class TableBuilder<TItem>
    {
        public TableBuilder<TItem> AddColumn(
            Expression<Func<TItem, object>> valueGetter,
            string sqlType = null,
            string columnName = null) => this;

        public TableBuilder<TItem> AddSubTable<TSubItem>(
            Expression<Func<TItem, IEnumerable<TSubItem>>> enumerableGetter,
            Action<SubTableBuilder<TSubItem, TItem>> subTableConsumer) => this;
    }

    class SubTableBuilder<TItem, TParentItem>
    {
        public SubTableBuilder<TItem, TParentItem> AddSubTable<TSubItem>(
            Expression<Func<TItem, IEnumerable<TSubItem>>> enumerableGetter,
            Action<SubTableBuilder<TSubItem, TItem>> subTableConsumer) => this;

        public SubTableBuilder<TItem, TParentItem> AddColumn(
            Expression<Func<TItem, object>> valueGetter,
            string sqlType = null,
            string customSqlColumnName = null) => this;
    }

    public class Person
    {
        public Guid Id { get; set; }
        public short Age { get; set; }
        [MaxLength(50)]
        public string FirstName { get; set; }
        [MaxLength(50)]
        public string LastName { get; set; }
        [MinLength(2)]
        [MaxLength(2)]
        public string CountryCode { get; set; }
        public Job[] Jobs { get; set; }
        public byte[] Logo { get; set; }
        public IEnumerable<byte> AnotherLogo { get; set; }
        public string[] Skills { get; set; }

        // Enumerable of basic types should be mapped to complex types with defined relations.
        public IEnumerable<Skill> SkillValues => Skills.Select(skill => new Skill(Id, skill));

        public Contact Contact { get; set; }
    }

    public class Job
    {
        public Guid PersonId { get; set; }
        public string CompanyName { get; set; }
        public string Address { get; set; }
    }

    public class Skill
    {
        public Skill(Guid personId, string tag)
        {
            PersonId = personId;
            Tag = tag;
        }

        public Guid PersonId { get; set; }
        public string Tag { get; set; }
    }

    public class Contact
    {
        public string Phone { get; set; }
        public string Email { get; set; }
    }

    class Program
    {
        static void Main()
        {
            new TableBuilder<Person>()
                .AddColumn(person => person.Id)
                .AddColumn(person => person.Age)
                .AddColumn(person => person.FirstName)
                .AddColumn(person => person.LastName)
                .AddColumn(person => person.CountryCode)
                .AddColumn(person => person.Logo)
                .AddColumn(person => person.AnotherLogo)
                .AddColumn(person => person.Contact.Phone)
                .AddColumn(person => person.Contact.Email)
                .AddSubTable(person => person.Jobs, jobBuilder => jobBuilder
                    .AddColumn(job => job.PersonId)
                    .AddColumn(job => job.CompanyName)
                    .AddColumn(job => job.Address)
                )
                .AddSubTable(person => person.SkillValues, skillBuilder => skillBuilder
                    .AddColumn(skill => skill.PersonId)
                    .AddColumn(skill => skill.Tag)
                );
        }
    }
}