using System;

namespace DataCloner.DataAccess
{
    internal class SqlTypeConverterMySql : AbstractSqlTypeConverter
    {
        protected override SqlType AnsiStringToSql()
        {
            return null;
        }
    }
}
