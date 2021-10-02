SqlTableDefinition(table) ::= "<\\>
CREATE TABLE <table.SqlTableName>
(
    <table.Columns:{c|<c.SqlColumnName> <c.SqlType>}; separator=\",\n\">
);
<table.SubTables:{t|<\n><SqlTableDefinition(t)>}>"