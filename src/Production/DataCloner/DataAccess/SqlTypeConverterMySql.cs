namespace DataCloner.DataAccess
{
    internal class SqlTypeConverterMySql : SqlTypeConverterBase
    {
        protected override SqlType AnsiStringToSql()
        {
            return null;
        }
    }
}
