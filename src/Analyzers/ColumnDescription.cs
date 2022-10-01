using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Coding4fun.DataTools.Analyzers.Extension;

namespace Coding4fun.DataTools.Analyzers
{
    [DebuggerDisplay("{ToString()}")]
    public class ColumnDescription: IAttributeHolder
    {
        internal readonly Dictionary<string, string> CustomAttributes = new (StringComparer.InvariantCultureIgnoreCase);

        internal string? SqlColumnName
        {
            get => CustomAttributes.GetValueOrNull(nameof(SqlColumnName));
            set => CustomAttributes.SetValue(nameof(SqlColumnName), value);
        }
        
        public ColumnDescription(string valueBody, string? propertyName = null, string? sqlType = null)
        {
            PropertyName = propertyName;
            SqlType = sqlType;
            ValueBody = valueBody;
        }

        public string? PropertyName { get; set; }
        public string? SqlType { get; }
        public string ValueBody { get; }
        public string? SharpType { get; set; }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        public override string ToString() => $"{nameof(PropertyName)}={PropertyName}," +
                                             $"{nameof(SqlType)}={SqlType}," +
                                             $"{nameof(ValueBody)}={ValueBody}";

        /// <inheritdoc />
        public IReadOnlyDictionary<string, string> Attributes => CustomAttributes;
    }
}