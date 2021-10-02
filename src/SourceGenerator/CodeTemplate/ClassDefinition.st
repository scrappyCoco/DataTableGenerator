ClassDefinition(newLine, usingNamespaces, class) ::= 
"<usingNamespaces:{ns|using <ns>;<\n>}>
namespace <class.Namespace>
{
    public partial class <class.SqlMappingClassName>
    {
        <DataTableDefinition(class.TableDescription)>
        
        public string GetSqlTableDefinition() => @\"
<SqlTableDefinition(class.TableDescription)><\\>
\";

        public void FillDataTables(IEnumerable\<<class.TableDescription.ClassName>> items)
        {
            <InvokeMethod(class.TableDescription, \"item\", \"items\")>
        }
        
        <MethodDefinition(class.TableDescription)>
    }
}"