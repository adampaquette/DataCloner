using System;
using DataCloner.DataClasse.Cache;

namespace DataCloner.DataAccess
{
    internal sealed class QueryHelperMySql : AbstractQueryHelper
    {
        public const string ProviderName = "MySql.Data.MySqlClient";

        private const string SqlGetDatabasesName =
        "SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA " +
        "WHERE SCHEMA_NAME NOT IN ('information_schema','performance_schema','mysql');";
        
        private const string SqlGetColumns = 
        "SELECT " +
            "'' AS SHEMA," +
            "TABLE_NAME," +
            "COLUMN_NAME," +
            "COLUMN_TYPE," +
            "COLUMN_KEY = 'PRI' AS 'IsPrimaryKey'," +
            "EXTRA = 'auto_increment' AS 'IsAutoIncrement' " +
        "FROM INFORMATION_SCHEMA.COLUMNS " +
        "WHERE TABLE_SCHEMA = @DATABASE " +
        "ORDER BY " +
            "TABLE_NAME," +
            "ORDINAL_POSITION;";

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
        "WHERE TC.TABLE_SCHEMA = @DATABASE " +
        "AND TC.CONSTRAINT_TYPE = 'FOREIGN KEY' " +
        "ORDER BY " +
            "TC.TABLE_NAME," +
            "TC.CONSTRAINT_NAME;";

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

        private const string SqlGetLastInsertedPk = "SELECT LAST_INSERT_ID();";

        private const string SqlEnforceIntegrityCheck = "SET UNIQUE_CHECKS=@ACTIVE; SET FOREIGN_KEY_CHECKS=@ACTIVE;";

        public QueryHelperMySql(Cache cache, string connectionString, Int16 serverId)
            : base(cache, ProviderName, connectionString, serverId, SqlGetDatabasesName,
                   SqlGetColumns, SqlGetForeignKey, SqlGetUniqueKey, SqlGetLastInsertedPk, 
                   SqlEnforceIntegrityCheck)
        {

        }      
    }
}
