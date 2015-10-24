namespace DataCloner.Data
{
    internal class SqlTypeConverterMsSql : SqlTypeConverterBase
    {
        protected override SqlType AnsiStringToSql()
        {
            return null;
        }
    }
}
