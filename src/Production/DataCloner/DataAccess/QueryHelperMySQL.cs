using System;
using DataCloner.DataClasse.Cache;
using DataCloner.Generator;

namespace DataCloner.DataAccess
{
    internal sealed class QueryHelperMySql : AbstractQueryHelper
    {
        private readonly static ISqlTypeConverter _typeConverter = new SqlTypeConverterMySql();
        private readonly static ISqlWriter _sqlWriter = new SqlWriterMySql();

        public const string ProviderName = "MySql.Data.MySqlClient";

        protected override string SqlGetDatabasesName =>
        "SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA " +
        "WHERE SCHEMA_NAME NOT IN ('information_schema','performance_schema','mysql');";

        protected override string SqlGetColumns =>
		"SELECT " +
			"'' AS SHEMA," +
			"COL.TABLE_NAME," +
			"COL.COLUMN_NAME," +
			"COL.COLUMN_TYPE," +
			"COL.COLUMN_KEY = 'PRI' AS 'IsPrimaryKey'," +
			"COL.EXTRA = 'auto_increment' AS 'IsAutoIncrement' " +
		"FROM INFORMATION_SCHEMA.COLUMNS COL " +
		"INNER JOIN INFORMATION_SCHEMA.TABLES TBL ON TBL.TABLE_CATALOG = COL.TABLE_CATALOG AND " +
													"TBL.TABLE_SCHEMA = COL.TABLE_SCHEMA AND " +
													"TBL.TABLE_NAME = COL.TABLE_NAME AND " +
													"TBL.TABLE_TYPE = 'BASE TABLE' " +
		"WHERE COL.TABLE_SCHEMA = @DATABASE " +
		"ORDER BY " +
			"COL.TABLE_NAME, " +
			"COL.ORDINAL_POSITION;";

        protected override string SqlGetForeignKeys =>
        "SELECT " +
            "'' AS 'Schema'," +
            "TC.TABLE_NAME," +
            "TC.CONSTRAINT_NAME," +
            "K.COLUMN_NAME," +
            "K.REFERENCED_TABLE_SCHEMA," +
            "K.REFERENCED_TABLE_NAME," +
            "K.REFERENCED_COLUMN_NAME " +
        "FROM information_schema.TABLE_CONSTRAINTS TC " +
        "INNER JOIN information_schema.KEY_COLUMN_USAGE K ON TC.TABLE_NAME = K.TABLE_NAME " +
                                                        "AND TC.CONSTRAINT_NAME = K.CONSTRAINT_NAME " +
		"INNER JOIN INFORMATION_SCHEMA.TABLES TBL ON TBL.TABLE_CATALOG = TC.CONSTRAINT_CATALOG AND " + 
											        "TBL.TABLE_SCHEMA = TC.TABLE_SCHEMA AND " +
											        "TBL.TABLE_NAME = TC.TABLE_NAME AND " +
											        "TBL.TABLE_TYPE = 'BASE TABLE' " +
        "WHERE TC.TABLE_SCHEMA = @DATABASE " +
        "AND TC.CONSTRAINT_TYPE = 'FOREIGN KEY' " +
        "ORDER BY " +
            "TC.TABLE_NAME," +
            "TC.CONSTRAINT_NAME;";

        protected override string SqlGetUniqueKeys =>
        "SELECT " +
            "'' AS 'Schema'," +
            "TC.TABLE_NAME," +
            "TC.CONSTRAINT_NAME," +
            "K.COLUMN_NAME " +
        "FROM information_schema.TABLE_CONSTRAINTS TC " +
        "INNER JOIN information_schema.KEY_COLUMN_USAGE K ON TC.TABLE_NAME = K.TABLE_NAME " +
                                                        "AND TC.CONSTRAINT_NAME = K.CONSTRAINT_NAME " +
        "WHERE TC.TABLE_SCHEMA = @DATABASE " +
        "AND TC.CONSTRAINT_TYPE = 'UNIQUE' " +
        "ORDER BY " +
            "TC.TABLE_NAME," +
            "TC.CONSTRAINT_NAME;";

        protected override string SqlGetLastInsertedPk => "SELECT LAST_INSERT_ID();";

        protected override string SqlEnforceIntegrityCheck => "SET UNIQUE_CHECKS=@ACTIVE; SET FOREIGN_KEY_CHECKS=@ACTIVE;";

        public override DbEngine Engine => DbEngine.MySql;
        public override ISqlTypeConverter TypeConverter => _typeConverter;
        public override ISqlWriter SqlWriter => _sqlWriter;

        public QueryHelperMySql(DatabasesSchema schema, string connectionString, Int16 serverId)
            : base(schema, ProviderName, connectionString, serverId)
        {

        }      
    }
}
