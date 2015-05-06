using System;

namespace DataCloner.DataAccess
{
    internal class MySqlTypeConverter : AbstractSqlTypeConverter
    {
        protected override SqlType AnsiStringToSql()
        {
            return null;
        }
    }
}
