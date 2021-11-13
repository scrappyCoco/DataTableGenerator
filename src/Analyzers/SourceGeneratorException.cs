using System;
using Microsoft.CodeAnalysis;

namespace Coding4fun.DataTools.Analyzers
{
    public class SourceGeneratorException: Exception
    {
        public string DiagnosticId { get; }
        public Location Location { get; }
        public SourceGeneratorException(string diagnosticId, string message, Location location): base(message)
        {
            DiagnosticId = diagnosticId;
            Location = location;
        }
    }
}