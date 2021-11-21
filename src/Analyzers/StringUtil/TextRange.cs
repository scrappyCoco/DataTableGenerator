using System;
using System.Diagnostics.CodeAnalysis;

namespace Coding4fun.DataTools.Analyzers.StringUtil
{
    public readonly struct TextRange
    {
        public int Offset { get; }
        public int Length { get; }

        [ExcludeFromCodeCoverage]
        public TextRange(int offset, int length)
        {
            if (offset < 0) throw new ArgumentException($"{nameof(offset)} must be greater than 0.", nameof(offset));
            if (length < 0) throw new ArgumentException($"{nameof(length)} must be greater than 0.", nameof(length));
            Offset = offset;
            Length = length;
        }
    }

}