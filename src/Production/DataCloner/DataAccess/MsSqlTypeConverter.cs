using System;

namespace DataCloner.DataAccess
{
    internal class MsSqlTypeConverter : AbstractSqlTypeConverter
    {
        protected override SqlType AnsiStringToSql()
        {
            return null;
        }
    }
}
