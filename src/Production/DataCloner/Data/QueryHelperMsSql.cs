using DataCloner.Core.Data.Generator;
using DataCloner.Core.Metadata;

namespace DataCloner.Core.Data
{
    internal sealed class QueryHelperMsSql : QueryHelperBase
    {
        public const string ProviderName = "System.Data.SqlClient";

        protected override string SqlGetDatabasesName =>
            "SELECT D.NAME FROM SYS.DATABASES D " +
            "WHERE D.NAME NOT IN ('master', 'tempdb');";
 
        protected override string SqlGetColumns =>
            "SELECT " +
            "    LOWER(COL.TABLE_SCHEMA), " +
            "    LOWER(COL.TABLE_NAME), " +
            "    LOWER(COL.COLUMN_NAME), " +
            "    LOWER(COL.DATA_TYPE), " +
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

        protected override string SqlGetForeignKeys =>
            "SELECT " +
            "    LOWER(TC.TABLE_SCHEMA), " +
            "    LOWER(TC.TABLE_NAME), " +
            "    LOWER(TC.CONSTRAINT_NAME), " +
            "    LOWER(KCU1.COLUMN_NAME), " +
            "    LOWER(KCU2.CONSTRAINT_NAME) AS REFERENCED_CONSTRAINT_NAME, " +
            "    LOWER(KCU2.TABLE_NAME) AS REFERENCED_TABLE_NAME, " +
            "    LOWER(KCU2.COLUMN_NAME) AS REFERENCED_COLUMN_NAME " +
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

        protected override string SqlGetUniqueKeys =>
            "SELECT " +
            "    LOWER(TC.CONSTRAINT_SCHEMA), " +
            "    LOWER(TC.TABLE_NAME), " +
            "    LOWER(TC.CONSTRAINT_NAME), " +
            "    LOWER(KCU.COLUMN_NAME) " +
            "FROM information_schema.TABLE_CONSTRAINTS TC " +
            "INNER JOIN information_schema.KEY_COLUMN_USAGE KCU ON KCU.CONSTRAINT_CATALOG = TC.CONSTRAINT_CATALOG " +
            "                                                  AND KCU.CONSTRAINT_SCHEMA = TC.CONSTRAINT_SCHEMA " +
            "                                                  AND KCU.CONSTRAINT_NAME = TC.CONSTRAINT_NAME " +
            "WHERE TC.TABLE_SCHEMA = @DATABASE " +
            "AND TC.CONSTRAINT_TYPE = 'UNIQUE' " +
            "ORDER BY " +
            "    TC.TABLE_NAME, " +
            "    TC.CONSTRAINT_NAME;";

        protected override string SqlGetLastInsertedPk =>
            "SELECT SCOPE_IDENTITY();";

        protected override string SqlEnforceIntegrityCheck =>
            "IF @ACTIVE = 1 BEGIN " +
            "    EXEC sp_MSForEachTable 'ALTER TABLE ? CHECK CONSTRAINT ALL' " +
            "END ELSE BEGIN " +
            "    EXEC sp_msforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT all;' " +
            "END;";

        public override DbEngine Engine => DbEngine.SqlServer; 
        public override ISqlTypeConverter TypeConverter { get; }
        public override ISqlWriter SqlWriter { get; }

        public QueryHelperMsSql(AppMetadata schema, string connectionString)
            : base(schema, ProviderName, connectionString)
        {
            TypeConverter = new SqlTypeConverterMsSql();
            SqlWriter = new SqlWriterMsSql();
        }
    }
}
