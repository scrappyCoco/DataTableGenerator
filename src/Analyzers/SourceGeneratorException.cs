using System;
using Microsoft.CodeAnalysis;

namespace Coding4fun.DataTools.Analyzers
{
    public class SourceGeneratorException: Exception
    {
        public Location Location { get; }
        public SourceGeneratorException(string message, Location location): base(message) => Location = location;
    }
}