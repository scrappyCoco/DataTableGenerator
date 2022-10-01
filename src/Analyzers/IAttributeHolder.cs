using System.Collections.Generic;

namespace Coding4fun.DataTools.Analyzers
{
    public interface IAttributeHolder
    {
        IReadOnlyDictionary<string, string> Attributes { get; }
    }
}