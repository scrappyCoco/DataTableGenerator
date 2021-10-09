using System;
using JetBrains.Annotations;

namespace Coding4fun.DataTableGenerator.Example
{
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
}