namespace DataCloner.Core.Data
{
    internal class MySqlTypeConverter : SqlTypeConverter
    {
        protected override SqlType AnsiStringToSql()
        {
            return null;
        }
    }
}
