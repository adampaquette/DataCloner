using System.Data;

namespace DataCloner.DataAccess
{
    public interface ISqlTypeConverter
    {
        SqlType ConvertToSql(DbType type);
        DbType ConvertFromSql(SqlType type);
    }
}
