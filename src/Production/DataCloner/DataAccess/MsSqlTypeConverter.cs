using System;

namespace DataCloner.DataAccess
{
    internal class MsSqlTypeConverter : AbstractSqlTypeConverter
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
