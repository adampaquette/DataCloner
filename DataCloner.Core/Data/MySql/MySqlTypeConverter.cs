namespace DataCloner.Core.Data.MySql
{
    internal class MySqlTypeConverter : SqlTypeConverter
    {
        protected override SqlType AnsiStringToSql()
        {
            return null;
        }
    }
}
