namespace DataCloner.DataAccess
{
    internal class SqlTypeConverterMsSql : SqlTypeConverterBase
    {
        protected override SqlType AnsiStringToSql()
        {
            return null;
        }
    }
}
