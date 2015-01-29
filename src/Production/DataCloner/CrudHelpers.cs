using DataCloner.DataAccess;
using DataCloner.DataClasse;

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

        public static void EnforceIntegrityCheck(this IServerIdentifier server, bool active)
        {
            QueryDispatcher.GetQueryHelper(server).EnforceIntegrityCheck(active);
        }
    }
}
