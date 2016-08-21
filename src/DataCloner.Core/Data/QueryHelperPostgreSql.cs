using DataCloner.Core.Data.Generator;
using DataCloner.Core.Metadata;

namespace DataCloner.Core.Data
{
    internal sealed class QueryHelperPostgreSql : QueryHelperBase
    {
        public const string ProviderName = "Npgsql";

        protected override string SqlGetDatabasesName =>
            "SELECT CATALOG_NAME FROM INFORMATION_SCHEMA.SCHEMATA " +
            "WHERE SCHEMA_NAME NOT LIKE 'pg_%' AND " +
            "SCHEMA_NAME NOT IN('information_schema');";

        //http://dba.stackexchange.com/questions/47098/how-do-i-determine-if-a-column-is-defined-as-a-serial-data-type-instead-of-an-in
        //http://stackoverflow.com/questions/1493262/list-all-sequences-in-a-postgres-db-8-1-with-sql
        //WITH fq_objects AS(SELECT c.oid, n.nspname || '.' ||c.relname AS fqname ,
        //                           c.relkind, c.relname AS relation
        //                    FROM pg_class c JOIN pg_namespace n ON n.oid = c.relnamespace),

        //     sequences AS(SELECT oid, fqname FROM fq_objects WHERE relkind = 'S'),
        //     tables AS(SELECT oid, fqname FROM fq_objects WHERE relkind = 'r')
        //SELECT
        //       s.fqname AS sequence,
        //       '->' as depends,
        //       t.fqname AS table,
        //       a.attname AS column
        //FROM
        //     pg_depend d JOIN sequences s ON s.oid = d.objid
        //                 JOIN tables t ON t.oid = d.refobjid
        //                 JOIN pg_attribute a ON a.attrelid = d.refobjid and a.attnum = d.refobjsubid
        //WHERE
        //     d.deptype = 'a' ;
        protected override string SqlGetColumns =>
        "SELECT " +
        "    COL.TABLE_SCHEMA AS Schema, " +
        "    COL.TABLE_NAME AS TableName, " +
        "    COL.COLUMN_NAME AS ColumnName, " +
        "    COL.DATA_TYPE AS DataType, " +
        "    COALESCE(COL.CHARACTER_MAXIMUM_LENGTH, COALESCE(COL.NUMERIC_PRECISION, COALESCE(COL.DATETIME_PRECISION, 0))) AS Precision, " +
        "    COALESCE(COL.NUMERIC_SCALE, 0) AS Scale, " +
        "    FALSE AS IsUnsigned, " +
        "    COALESCE(( " +
        "        SELECT TRUE " +
        "        FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE KCU " +
        "        INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS TC ON TC.CONSTRAINT_CATALOG = KCU.CONSTRAINT_CATALOG AND " +
        "                                  TC.CONSTRAINT_SCHEMA = KCU.CONSTRAINT_SCHEMA AND " +
        "                                  TC.CONSTRAINT_NAME = KCU.CONSTRAINT_NAME " +
        "        WHERE " +
        "        KCU.TABLE_CATALOG = COL.TABLE_CATALOG AND " +
        "        KCU.TABLE_SCHEMA = COL.TABLE_SCHEMA AND " +
        "        KCU.TABLE_NAME = COL.TABLE_NAME AND " +
        "        KCU.COLUMN_NAME = COL.COLUMN_NAME AND " +
        "        TC.CONSTRAINT_TYPE = 'PRIMARY KEY' " +
        "        LIMIT 1 " +
        "    ), FALSE) AS IsPrimaryKey, " +
        "    CASE WHEN COLUMN_DEFAULT ~ '^nextval' THEN TRUE ELSE FALSE END AS IsAutoIncrement " +
        "FROM INFORMATION_SCHEMA.COLUMNS COL " +
        "INNER JOIN INFORMATION_SCHEMA.TABLES TBL ON TBL.TABLE_CATALOG = COL.TABLE_CATALOG AND " +
        "                                            TBL.TABLE_SCHEMA = COL.TABLE_SCHEMA AND " +
        "                                            TBL.TABLE_NAME = COL.TABLE_NAME AND " +
        "                                            TBL.TABLE_TYPE = 'BASE TABLE' " +
        "WHERE " + 
        "    COL.TABLE_CATALOG = @DATABASE AND " +
        "    COL.TABLE_SCHEMA NOT LIKE 'pg_%' AND " +
        "    COL.TABLE_SCHEMA NOT IN('information_schema') " +
        "ORDER BY " +
        "    COL.TABLE_NAME, " +
        "    COL.ORDINAL_POSITION;";

        protected override string SqlGetForeignKeys =>
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

        protected override string SqlGetUniqueKeys =>
            "SELECT " +
            "    TC.CONSTRAINT_SCHEMA, " +
            "    TC.TABLE_NAME, " +
            "    TC.CONSTRAINT_NAME, " +
            "    KCU.COLUMN_NAME " +
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
            "SELECT CURRVAL('kjhkjhr_id_seq');";

        protected override string SqlEnforceIntegrityCheck
        {
            get { throw new System.NotImplementedException(); }
        }        

        public override DbEngine Engine => DbEngine.PostgreSql;
        public override ISqlTypeConverter TypeConverter { get; }
        public override ISqlWriter SqlWriter { get; }

        public QueryHelperPostgreSql(ExecutionContextMetadata schema, string connectionString)
            : base(schema, ProviderName, connectionString)
        {
            TypeConverter = new SqlTypeConverterPostgreSql();
            SqlWriter = new SqlWriterPostgreSql();
        }
    }
}