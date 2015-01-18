using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DataCloner.DataAccess;
using DataCloner.DataClasse.Cache;
using DataCloner.Framework.GeneralExtensionHelper;

namespace DataCloner
{
    internal static class CrudHelpers
    {
        public static object[][] Select(this IRowIdentifier ri)
        {
            return QueryDispatcher.GetQueryHelper(ri).Select(ri);
        }

        public static void Insert(this ITableIdentifier ti, object[] data)
        {
            QueryDispatcher.GetQueryHelper(ti).Insert(ti, data);
        }

        public static object GetLastInsertedPk(this IServerIdentifier server)
        {
            return QueryDispatcher.GetQueryHelper(server).GetLastInsertedPk();
        }
    }
}
