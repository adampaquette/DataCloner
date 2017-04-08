namespace DataCloner.Core.Data.PostgreSql
{
    internal class PostgreSqlTypeConverter : SqlTypeConverter
    {
        protected override SqlType AnsiStringToSql()
        {
            return null;
        }
    }
}
