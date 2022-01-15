
using Coding4fun.DataTools.Analyzers.StringUtil;
using Coding4fun.DataTools.Analyzers;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Coding4fun.DataTools.Test.TestData.SourceGenerator
{
    public class PersonSqlMapping : MsSqlBatchHandler
    {
        public readonly DataTable _PersonDataTable;
        public readonly DataTable _JobDataTable;
        public readonly DataTable _SkillDataTable;



        public PersonSqlMapping(MsSqlConfig config) : base(config, CreateDataTables()) => _logoutDt = DataTables[0];

        /// <inheritdoc />
        protected override string TempTableDeclaration => @"CREATE TABLE #MY_PERSON
(
  id UNIQUEIDENTIFIER,
  age SMALLINT,
  first_name NVARCHAR(50),
  last_name NVARCHAR(50),
  country NCHAR(2),
  phone NVARCHAR(MAX),
  photo VARBINARY(MAX)
);
CREATE TABLE #job
(
  person_id UNIQUEIDENTIFIER,
  company_name VARCHAR(100),
  address VARCHAR(200)
);
CREATE TABLE #skill
(
  person_id UNIQUEIDENTIFIER,
  tag VARCHAR(100)
);
";

    /// <inheritdoc />
    protected override string ProcedureName => "dbo.InsertPerson";

    private static DataTable[] CreateDataTables()
    {
        return new[] {  _PersonDataTable,
        public readonly DataTable _JobDataTable;
        public readonly DataTable _SkillDataTable;
    };
    }

    /// <inheritdoc />
    protected override void AddEntityInternal(object entity)
    {
        var login = (LoginMessage)entity;
        _logoutDt.Rows.Add(login.Email, login.Time, login.SessionId);
    }

}
