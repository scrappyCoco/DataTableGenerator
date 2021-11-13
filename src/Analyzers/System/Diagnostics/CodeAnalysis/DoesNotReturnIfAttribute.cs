using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace System.Diagnostics.CodeAnalysis
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class DoesNotReturnIf : Attribute
    {
        public bool ParameterValue { get; }

        public DoesNotReturnIf(bool parameterValue) => ParameterValue = parameterValue;
    }
}