// using System.Collections.Generic;
// using System.Linq;
//
// namespace Coding4fun.DataTools.Analyzers.Template.TableBuilder
// {
//     internal class BodyResolver: ResolverBase
//     {
//         private readonly string _linePrefix;
//
//         public BodyResolver(string linePrefix) => _linePrefix = linePrefix;
//
//         public override object?[] Resolve(CodeTemplate template, IEnumerable<object> contextObjects)
//         {
//             if (template.Name == "columns")
//             {
//                 var tableDescription = (TableDescription)contextObjects.Last();
//                 return tableDescription.Columns.Cast<object>().ToArray();
//             }
//             
//             if (template.Name == "subTables")
//             {
//                 var tableDescription = (TableDescription)contextObjects.Last();
//                 return tableDescription.SubTables.Cast<object>().ToArray();
//             }
//             
//             if (template.Name == "parentTableVarName")
//             {
//                 var tableDescription = (TableDescription)contextObjects.Last();
//                 return new object?[] { tableDescription.ParentTable?.VarName };
//             }
//             
//             if (template.Name == "enumerableName")
//             {
//                 var tableDescription = (TableDescription)contextObjects.Last();
//                 return new object?[] { tableDescription.EnumerableName };
//             }
//             
//             if (template.Name == "childTableVarName")
//             {
//                 var tableDescription = (TableDescription)contextObjects.Last();
//                 return new object?[] { tableDescription.VarName };
//             }
//
//             if (template.Name == "varName")
//             {
//                 var tableDescription = (TableDescription)contextObjects.Reverse().Skip(1).First();
//                 return new object?[] { tableDescription.VarName };
//             }
//             
//             if (template.Name == "valueBody")
//             {
//                 var columnDescription = (ColumnDescription)contextObjects.Last();
//                 return new object?[] { columnDescription.ValueBody };
//             }
//
//             if (template.Name == "space")
//             {
//                 int spaceCount = (contextObjects.Count() - 1) * 4;
//                 return new object?[] { _linePrefix + new string(' ', spaceCount) };
//             }
//
//             return new object?[] { null };
//         }
//
//         /// <inheritdoc />
//         public override bool TryReplaceTemplate(CodeTemplate currentTemplate, out ResolverBase? newResolver,
//             LinkedList<object> contextObjects)
//         {
//             newResolver = null;
//             
//             if (currentTemplate.Name == "body")
//             {
//                 newResolver = this;
//                 return true;
//             }
//
//             return false;
//         }
//     }
// }