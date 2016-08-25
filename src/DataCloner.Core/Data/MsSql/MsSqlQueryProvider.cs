using DataCloner.Core.Data.Generator;

namespace DataCloner.Core.Data.MsSql
{
    internal sealed class MsSqlQueryProvider : QueryProvider
    {
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

        public MsSqlQueryProvider()
        {
            TypeConverter = new MsSqlTypeConverter();
            SqlWriter = new SqlWriterMsSql();
        }
    }
}
