namespace DataCloner.Core.Data.MsSql
{
    internal class MsSqlTypeConverter : SqlTypeConverter
    {
        protected override SqlType AnsiStringToSql()
        {
            return null;
        }
    }
}
