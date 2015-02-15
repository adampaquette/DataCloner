using DataCloner.DataClasse;

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
