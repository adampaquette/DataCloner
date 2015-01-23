using System.Data;
using DataCloner.DataClasse.Cache;

namespace DataCloner.PlugIn
{
    public interface IDataBuilder
    {
       object BuildData(IDbConnection conn, DbEngine engine, ITableSchema table, IColumnDefinition column);
    }
}
