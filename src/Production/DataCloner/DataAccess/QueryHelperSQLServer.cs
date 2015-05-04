using System;
using DataCloner.DataClasse.Cache;

namespace DataCloner.DataAccess
{
    internal sealed class QueryHelperSqlServer : AbstractQueryHelper
    {
        public const string ProviderName = "System.Data.SqlClient";

        private const string SqlGetDatabasesName =
        "SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA " +
		"WHERE SCHEMA_NAME NOT IN ('information_schema','sys') AND " +
		"SCHEMA_NAME NOT LIKE 'db\_%' ESCAPE '\';";

        private const string SqlGetColumns =
        "SELECT " +
		"    COL.TABLE_SCHEMA, " +
		"    COL.TABLE_NAME, " +
		"    COL.COLUMN_NAME, " +
		"    COL.DATA_TYPE, " +
		"    COLUMNPROPERTY(object_id(COL.TABLE_NAME), COL.COLUMN_NAME, 'Precision') AS 'Precision',  " +
		"    COLUMNPROPERTY(object_id(COL.TABLE_NAME), COL.COLUMN_NAME, 'Scale') AS 'Scale',  " +
		"    ISNULL(( " +
		"        SELECT TOP 1 1 " +
		"        FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE KCU " +
		"        INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS TC ON TC.CONSTRAINT_CATALOG = KCU.CONSTRAINT_CATALOG AND " +
		"                                                              TC.CONSTRAINT_SCHEMA = KCU.CONSTRAINT_SCHEMA AND " +
		"                                                              TC.CONSTRAINT_NAME = KCU.CONSTRAINT_NAME " +
		"        WHERE " +
		"            KCU.TABLE_CATALOG = COL.TABLE_CATALOG AND " +
		"            KCU.TABLE_SCHEMA = COL.TABLE_SCHEMA AND " +
		"            KCU.TABLE_NAME = COL.TABLE_NAME AND " +
		"            KCU.COLUMN_NAME = COL.COLUMN_NAME AND " +
		"            TC.CONSTRAINT_TYPE = 'PRIMARY KEY' " +
		"    ), 0) AS 'IsPrimaryKey', " +
		"    COLUMNPROPERTY(object_id(COL.TABLE_NAME), COL.COLUMN_NAME, 'IsIdentity') AS 'IsAutoIncrement' " +
		"FROM INFORMATION_SCHEMA.COLUMNS COL " +
		"INNER JOIN INFORMATION_SCHEMA.TABLES TBL ON TBL.TABLE_CATALOG = COL.TABLE_CATALOG AND " +
		"                                            TBL.TABLE_SCHEMA = COL.TABLE_SCHEMA AND " +
		"                                            TBL.TABLE_NAME = COL.TABLE_NAME AND " +
		"                                            TBL.TABLE_TYPE = 'BASE TABLE' " +
		"WHERE COL.TABLE_SCHEMA = @DATABASE " +
		"ORDER BY " +
		"    COL.TABLE_NAME, " +
		"    COL.ORDINAL_POSITION;";

        //TODO
        private const string SqlGetForeignKey =
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

        //TODO
        private const string SqlGetUniqueKey =
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

        private const string SqlGetLastInsertedPk = "SELECT SCOPE_IDENTITY();";

        //Disable constraints for all tables:
        //EXEC sp_msforeachtable "ALTER TABLE ? NOCHECK CONSTRAINT all"
        //Re-enable constraints for all tables:
        //EXEC sp_msforeachtable "ALTER TABLE ? WITH CHECK CHECK CONSTRAINT all"
        //TODO
        private const string SqlEnforceIntegrityCheck = "SET UNIQUE_CHECKS=@ACTIVE; SET FOREIGN_KEY_CHECKS=@ACTIVE;";

        public QueryHelperSqlServer(Cache cache, string connectionString, Int16 serverId)
            : base(cache, ProviderName, connectionString, serverId, SqlGetDatabasesName,
                   SqlGetColumns, SqlGetForeignKey, SqlGetUniqueKey, SqlGetLastInsertedPk,
                   SqlEnforceIntegrityCheck)
        {

        }
    }
}
