using DataCloner.Core.Data.Generator;
using DataCloner.Core.Data.Generator.PostgreSql;

namespace DataCloner.Core.Data.PostgreSql
{
    internal sealed class PostgreSqlQueryProvider : QueryProvider
    {
        private static PostgreSqlQueryProvider _instance;

        public const string ProviderName = "Npgsql";

        protected override string SqlGetLastInsertedPk =>
            "SELECT CURRVAL('kjhkjhr_id_seq');";

        protected override string SqlEnforceIntegrityCheck
        {
            get { throw new System.NotImplementedException(); }
        }        

        public override DbEngine Engine => DbEngine.PostgreSql;
        public override ISqlWriter SqlWriter { get; }

        public static PostgreSqlQueryProvider Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new PostgreSqlQueryProvider();
                return _instance;
            }
        }

        public PostgreSqlQueryProvider()
        {
            SqlWriter = new PostgreSqlWriter();
        }
    }
}