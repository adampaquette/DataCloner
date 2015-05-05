using System;
using System.Data;

namespace DataCloner.DataAccess
{
    interface ISqlTypeConverter
    {
        SqlType ConvertToSql(DbType type);
        DbType ConvertFromSql(SqlType type);
    }
}
