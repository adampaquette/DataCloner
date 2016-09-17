namespace DataCloner.Core
{
    public static class ConnectionsContextExtensions
    {
        #region QueryProvider

        public static object[][] Select(this ConnectionsContext connectionsContext, RowIdentifier row)
        {
            var ctx = connectionsContext[row];
            return ctx.QueryProvider.Select(ctx.Connection, connectionsContext.Metadatas, row);
        }


        //public static void EnforceIntegrityCheck(this IQueryProxy dispatcher, SehemaIdentifier schema, bool active)
        //{
        //    var ctx = dispatcher[schema];
        //    ctx.QueryProvider.EnforceIntegrityCheck(ctx.Connection, active);
        //}

        //public static void Execute(this IQueryProxy dispatcher, SehemaIdentifier schema, ExecutionPlan plan)
        //{
        //    var ctx = dispatcher[schema];
        //    ctx.QueryProvider.Execute(ctx.Connection, ctx.Metadatas, plan);
        //}

        //public static object GetLastInsertedPk(this IQueryProxy dispatcher, short serverId)
        //{
        //    var ctx = dispatcher[serverId];
        //    return ctx.QueryProvider.GetLastInsertedPk(ctx.Connection);
        //}

        #endregion

        #region MetadataProvider

        //public static string[] GetDatabasesName(this IQueryProxy dispatcher, short serverId)
        //{
        //    var ctx = dispatcher[serverId];
        //    return ctx.MetadataProvider.GetDatabasesName(ctx.Connection);
        //}

        //public static void LoadColumns(this IQueryProxy dispatcher, short serverId, string database)
        //{
        //    var ctx = dispatcher[serverId];
        //    ctx.MetadataProvider.LoadColumns(ctx.Connection, ctx.Metadatas, serverId, database);
        //}

        //public static void LoadForeignKeys(this IQueryProxy dispatcher, short serverId, string database)
        //{
        //    var ctx = dispatcher[serverId];
        //    ctx.MetadataProvider.LoadForeignKeys(ctx.Connection, ctx.Metadatas, serverId, database);
        //}

        //public static void LoadUniqueKeys(this IQueryProxy dispatcher, short serverId, string database)
        //{
        //    var ctx = dispatcher[serverId];
        //    ctx.MetadataProvider.LoadUniqueKeys(ctx.Connection, ctx.Metadatas, serverId, database);
        //}

        #endregion
    }
}
