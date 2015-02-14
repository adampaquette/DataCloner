using DataCloner.DataClasse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataCloner.DataAccess
{
    internal static class QueryDispatcherExtensions
    {
        public static void Insert(this QueryDispatcher dispatcher, ITableIdentifier table, object[] row)
        {
            dispatcher.GetQueryHelper(table).Insert(table, row);
        }

        public static object[][] Select(this QueryDispatcher dispatcher, IRowIdentifier row)
        {
            return dispatcher.GetQueryHelper(row).Select(row);
        }
    }
}
