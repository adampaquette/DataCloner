using DataCloner.DataClasse.Cache;
using SqlFu;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataCloner.PlugIn
{
    public interface IDataBuilder
    {
       object BuildData(IDbConnection conn, ITableSchema table, IColumnDefinition column);
    }
}
