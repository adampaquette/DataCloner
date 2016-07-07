namespace DataCloner.Core.Data
{
    internal class SqlTypeConverterPostgreSql : SqlTypeConverterBase
    {
        protected override SqlType AnsiStringToSql()
        {
            return null;
        }
    }
}
