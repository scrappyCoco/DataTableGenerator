using System.Collections.Generic;
using JetBrains.Annotations;

namespace Coding4fun.DataTableGenerator.Entity
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class Person
    {
        public short Age { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CountryCode { get; set; }
        public List<Job> Jobs { get; set; }
        public List<string> Skills { get; set; }
    }
}