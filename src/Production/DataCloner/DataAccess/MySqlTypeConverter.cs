using System;

namespace DataCloner.DataAccess
{
    internal class MySqlTypeConverter : AbstractSqlTypeConverter
    {
        protected override bool AnsiStringFixedLengthFromSql(SqlType t)
        {
            return false;
        }

        protected override bool AnsiStringFromSql(SqlType t)
        {
            return false;
        }

        protected override SqlType AnsiStringToSql()
        {
            return null;
        }
    }
}
