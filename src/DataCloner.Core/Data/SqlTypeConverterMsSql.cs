namespace DataCloner.Core.Data
{
    internal class SqlTypeConverterMsSql : SqlTypeConverterBase
    {
        protected override SqlType AnsiStringToSql()
        {
            return null;
        }
    }
}
