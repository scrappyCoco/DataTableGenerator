DataTableDefinition(tableDescription) ::= "<\\>
public DataTable <tableDescription.DataTableName> { get; } = new DataTable();<\\>
<tableDescription.SubTables:{t|<\n><DataTableDefinition(t)>}>"