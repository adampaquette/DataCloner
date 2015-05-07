using System;
using DataCloner.DataClasse.Cache;

namespace DataCloner.DataAccess
{
    internal sealed class QueryHelperSqlServer : AbstractQueryHelper
    {
        public const string ProviderName = "System.Data.SqlClient";

        private const string SqlGetDatabasesName =
        "SELECT CATALOG_NAME FROM INFORMATION_SCHEMA.SCHEMATA " +
		"WHERE SCHEMA_NAME NOT IN ('information_schema','sys') AND " +
		"SCHEMA_NAME NOT LIKE 'db\\_%' ESCAPE '\\';";

        private const string SqlGetColumns =
        "SELECT " +
		"    COL.TABLE_SCHEMA, " +
		"    COL.TABLE_NAME, " +
		"    COL.COLUMN_NAME, " +
		"    COL.DATA_TYPE, " +
		"    COLUMNPROPERTY(object_id(COL.TABLE_NAME), COL.COLUMN_NAME, 'Precision') AS 'Precision',  " +
		"    ISNULL(COLUMNPROPERTY(object_id(COL.TABLE_NAME), COL.COLUMN_NAME, 'Scale'), 0) AS 'Scale',  " +
        "    CAST(0 AS BIT) AS IsUnsigned, " +
		"    CAST(ISNULL(( " +
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
		"    ), 0) AS BIT) AS 'IsPrimaryKey', " +
		"    CAST(COLUMNPROPERTY(object_id(COL.TABLE_NAME), COL.COLUMN_NAME, 'IsIdentity') AS BIT) AS 'IsAutoIncrement' " +
		"FROM INFORMATION_SCHEMA.COLUMNS COL " +
		"INNER JOIN INFORMATION_SCHEMA.TABLES TBL ON TBL.TABLE_CATALOG = COL.TABLE_CATALOG AND " +
		"                                            TBL.TABLE_SCHEMA = COL.TABLE_SCHEMA AND " +
		"                                            TBL.TABLE_NAME = COL.TABLE_NAME AND " +
		"                                            TBL.TABLE_TYPE = 'BASE TABLE' " +
        "WHERE COL.TABLE_CATALOG = @DATABASE " +
		"ORDER BY " +
		"    COL.TABLE_NAME, " +
		"    COL.ORDINAL_POSITION;";

        private const string SqlGetForeignKey =
        "SELECT " +
        "    TC.TABLE_SCHEMA, " +
        "    TC.TABLE_NAME, " +
        "    TC.CONSTRAINT_NAME, " +
        "    KCU1.COLUMN_NAME, " +
        "    KCU2.CONSTRAINT_NAME AS REFERENCED_CONSTRAINT_NAME, " +
        "    KCU2.TABLE_NAME AS REFERENCED_TABLE_NAME, " +
        "    KCU2.COLUMN_NAME AS REFERENCED_COLUMN_NAME " +
        "FROM information_schema.TABLE_CONSTRAINTS TC " +
        "INNER JOIN INFORMATION_SCHEMA.TABLES TBL ON TBL.TABLE_CATALOG = TC.TABLE_CATALOG AND " +
        "                                            TBL.TABLE_SCHEMA = TC.TABLE_SCHEMA AND " +
        "                                            TBL.TABLE_NAME = TC.TABLE_NAME AND " +
        "                                            TBL.TABLE_TYPE = 'BASE TABLE' " +
        "INNER JOIN information_schema.KEY_COLUMN_USAGE KCU1 ON KCU1.CONSTRAINT_CATALOG = TC.CONSTRAINT_CATALOG " +
        "                                                   AND KCU1.CONSTRAINT_SCHEMA = TC.CONSTRAINT_SCHEMA " +
        "                                                   AND KCU1.CONSTRAINT_NAME = TC.CONSTRAINT_NAME " +
        "INNER JOIN INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS AS RC ON RC.CONSTRAINT_CATALOG = KCU1.CONSTRAINT_CATALOG " +
        "                                                           AND RC.CONSTRAINT_SCHEMA  = KCU1.CONSTRAINT_SCHEMA " +
        "                                                           AND RC.CONSTRAINT_NAME = KCU1.CONSTRAINT_NAME " +
        "INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE KCU2 ON KCU2.CONSTRAINT_CATALOG = RC.UNIQUE_CONSTRAINT_CATALOG " +
        "                                                   AND KCU2.CONSTRAINT_SCHEMA = RC.UNIQUE_CONSTRAINT_SCHEMA " +
        "                                                   AND KCU2.CONSTRAINT_NAME = RC.UNIQUE_CONSTRAINT_NAME " +
        "                                                   AND KCU2.ORDINAL_POSITION = KCU1.ORDINAL_POSITION " +
        "WHERE TC.TABLE_CATALOG = @DATABASE " +
        "  AND TC.CONSTRAINT_TYPE = 'FOREIGN KEY' " +
        "ORDER BY " +
        "    TC.TABLE_NAME, " +
        "    TC.CONSTRAINT_NAME;";

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

        private readonly static ISqlTypeConverter _typeConverter = new MsSqlTypeConverter();
        public override ISqlTypeConverter TypeConverter => _typeConverter;

        public QueryHelperSqlServer(Cache cache, string connectionString, Int16 serverId)
            : base(cache, ProviderName, connectionString, serverId, SqlGetDatabasesName,
                   SqlGetColumns, SqlGetForeignKey, SqlGetUniqueKey, SqlGetLastInsertedPk,
                   SqlEnforceIntegrityCheck)
        {

        }
    }
}
