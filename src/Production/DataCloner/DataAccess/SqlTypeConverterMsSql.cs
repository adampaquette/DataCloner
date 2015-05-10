using System;

namespace DataCloner.DataAccess
{
    internal class SqlTypeConverterMsSql : AbstractSqlTypeConverter
    {
        protected override SqlType AnsiStringToSql()
        {
            return null;
        }
    }
}
