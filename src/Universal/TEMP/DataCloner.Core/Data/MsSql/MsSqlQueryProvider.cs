using DataCloner.Core.Data.Generator;
using DataCloner.Core.Data.Generator.MsSql;

namespace DataCloner.Core.Data.MsSql
{
    internal sealed class MsSqlQueryProvider : QueryProvider 
    {
        private static MsSqlQueryProvider _instance;

        public const string ProviderName = "System.Data.SqlClient";

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

        public static MsSqlQueryProvider Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new MsSqlQueryProvider();
                return _instance;
            }
        }

        public MsSqlQueryProvider()
        {
            TypeConverter = new MsSqlTypeConverter();
            SqlWriter = new MsSqlWriter();
        }
    }
}
