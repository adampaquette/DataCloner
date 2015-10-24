using System.Data;

namespace DataCloner.Data
{
    public interface ISqlTypeConverter
    {
        SqlType ConvertToSql(DbType type);
        DbType ConvertFromSql(SqlType type);
    }
}
