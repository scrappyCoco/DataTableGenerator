using JetBrains.Annotations;

namespace Coding4fun.DataTableGenerator.Entity
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class Job
    {
        public string CompanyName { get; set; }
        public string Address { get; set; }
    }
}