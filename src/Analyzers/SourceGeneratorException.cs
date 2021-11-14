using System;
using Microsoft.CodeAnalysis;
using Message = System.Collections.Generic.KeyValuePair<string, string>;

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
        
        public SourceGeneratorException(Message message): base(message.Value)
        {
            DiagnosticId = message.Key;
            Location = Location.None;
        }
    }
}