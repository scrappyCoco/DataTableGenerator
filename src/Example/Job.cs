using System;
using JetBrains.Annotations;

namespace Coding4fun.DataTools.Example
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class Job
    {
        public Guid PersonId { get; set; }
        public string CompanyName { get; set; }
        public string Address { get; set; }
    }
}