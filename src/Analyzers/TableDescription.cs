using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Coding4fun.DataTools.Analyzers.Extension;
using JetBrains.Annotations;

namespace Coding4fun.DataTools.Analyzers
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class TableDescription
    {
        internal readonly Dictionary<string, string> CustomAttributes = new (StringComparer.InvariantCultureIgnoreCase);

        internal string? SqlTableName
        {
            get => CustomAttributes.GetValueOrNull(nameof(SqlTableName));
            set => CustomAttributes.SetValue(nameof(SqlTableName), value);
        }

        public TableDescription(string className)
        {
            ClassName = className;
            Columns = new List<ColumnDescription>();
            SubTables = new List<TableDescription>();
        }

        public string? ClassName { get; }
        public string? EnumerableName { get; set; }
        public string[] PreExecutionActions { get; set; } = Array.Empty<string>();
        public TableDescription? ParentTable { get; set; }
        
        public List<ColumnDescription> Columns { get; }
        public List<TableDescription> SubTables { get; }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        public override string ToString() => $"{nameof(ClassName)}={ClassName}," +
                                             $"{nameof(EnumerableName)}={EnumerableName},";
    }
}