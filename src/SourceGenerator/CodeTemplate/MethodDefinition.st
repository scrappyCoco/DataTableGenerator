MethodDefinition(table) ::= "
public void Add<table.EntityName>(<table.ClassName> <table.VarName>)
{
    <table.DataTableName>.Rows.Add(
        <table.Columns:{c|<c.ValueBody>};separator=\",\n\">
    );
}<\\>
<table.SubTables:{t|<\n><\n><MethodDefinition(t)>}>"