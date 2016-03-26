namespace DataCloner.Core.Data
{
    internal class SqlTypeConverterMySql : SqlTypeConverterBase
    {
        protected override SqlType AnsiStringToSql()
        {
            return null;
        }
    }
}
