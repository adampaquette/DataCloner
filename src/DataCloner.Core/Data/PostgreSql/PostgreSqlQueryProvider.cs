using DataCloner.Core.Data.Generator;

namespace DataCloner.Core.Data.PostgreSql
{
    internal sealed class PostgreSqlQueryProvider : QueryProvider
    {
        public const string ProviderName = "Npgsql";

        protected override string SqlGetLastInsertedPk =>
            "SELECT CURRVAL('kjhkjhr_id_seq');";

        protected override string SqlEnforceIntegrityCheck
        {
            get { throw new System.NotImplementedException(); }
        }        

        public override DbEngine Engine => DbEngine.PostgreSql;
        public override ISqlTypeConverter TypeConverter { get; }
        public override ISqlWriter SqlWriter { get; }

        public PostgreSqlQueryProvider()
        {
            TypeConverter = new PostgreSqlTypeConverter();
            SqlWriter = new SqlWriterPostgreSql();
        }
    }
}