InvokeMethod(table, varName, items) ::= "
foreach (var <varName> in <items>)
{
    Add<table.EntityName>(<varName>);
    <table.SubTables:{t|<InvokeMethod(t, t.VarName, t.EnumerableName)>}; separator=\"\n\">
}"