using System.Data;

namespace DataCloner.Core.Data
{
    public interface ISqlTypeConverter
    {
        SqlType ConvertToSql(DbType type);

        DbType ConvertFromSql(SqlType type);
    }
}
