using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using JetBrains.Annotations;

namespace Coding4fun.DataTools.Example
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class Person
    {
        public Guid Id { get; set; }
        public short Age { get; set; }
        [MaxLength(50)] public string FirstName { get; set; }
        [MaxLength(50)] public string LastName { get; set; }
        [MinLength(2)] [MaxLength(2)] public string CountryCode { get; set; }
        public Job[] Jobs { get; set; }
        public byte[] Logo { get; set; }
        public string[] Skills { get; set; }

        // Enumerable of basic types should be mapped to complex types with defined relations.
        public IEnumerable<Skill> SkillValues => Skills.Select(skill => new Skill(Id, skill));

        public Contact Contact { get; set; }
    }
}