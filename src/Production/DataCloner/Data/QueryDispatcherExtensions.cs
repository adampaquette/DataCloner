using DataCloner.Internal;

namespace DataCloner.Data
{
    internal static class QueryDispatcherExtensions
    {
        //public static void Insert(this IQueryDispatcher dispatcher, TableIdentifier table, object[] row)
        //{
        //    dispatcher.GetQueryHelper(table).Insert(table, row);
        //}

        public static object[][] Select(this IQueryDispatcher dispatcher, RowIdentifier row)
        {
            return dispatcher.GetQueryHelper(row).Select(row);
        }
    }
}
