using System;

using DataCloner.DataClasse.Cache;

namespace DataCloner.DataAccess
{
    internal sealed class QueryHelperMySql : AbstractQueryHelper
    {
        public const string ProviderName = "MySql.Data.MySqlClient";

        private const string _SQL_GET_DATABASES_NAME =
        "SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA " +
        "WHERE SCHEMA_NAME NOT IN ('information_schema','performance_schema','mysql');";
        
        private const string _SQL_GET_COLUMNS = 
        "SELECT " +
            "'' AS SHEMA," +
            "TABLE_NAME," +
            "COLUMN_NAME," +
            "COLUMN_TYPE," +
            "COLUMN_KEY = 'PRI' AS 'IsPrimaryKey'," +
            "0 AS 'IsForeignKey'," +
            "0 AS 'IsUniqueKey'," +
            "EXTRA = 'auto_increment' AS 'IsAutoIncrement' " +
        "FROM INFORMATION_SCHEMA.COLUMNS " +
        "WHERE TABLE_SCHEMA = @DATABASE " +
        "ORDER BY " +
            "TABLE_NAME," +
            "ORDINAL_POSITION;";

        private const string _SQL_GET_FOREIGN_KEY =
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

        private const string _SQL_GET_UNIQUE_KEY =
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

        private const string _SQL_GET_LAST_INSERTED_PK = "SELECT LAST_INSERT_ID();";

        private const string _SQL_ENFORCE_INTEGRITY_CHECK = "SET UNIQUE_CHECKS=@ACTIVE; SET FOREIGN_KEY_CHECKS=@ACTIVE;";

        public QueryHelperMySql(string connectionString, Int16 serverId, Cache cache)
            : base(ProviderName, connectionString, serverId, cache, _SQL_GET_DATABASES_NAME,
                   _SQL_GET_COLUMNS, _SQL_GET_FOREIGN_KEY, _SQL_GET_UNIQUE_KEY, _SQL_GET_LAST_INSERTED_PK, 
                   _SQL_ENFORCE_INTEGRITY_CHECK)
        {

        }      
    }
}
