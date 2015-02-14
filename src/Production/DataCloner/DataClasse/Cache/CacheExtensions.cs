namespace DataCloner.DataClasse.Cache
{
    internal static class CacheExtensions
    {
        public static TableSchema GetTable(this Cache cache, IForeignKey fk)
        {
            return cache.DatabasesSchema.GetTable(fk.ServerIdTo, fk.DatabaseTo, fk.SchemaTo, fk.TableTo);
        }

        public static TableSchema GetTable(this Cache cache, IDerivativeTable dt)
        {
            return cache.DatabasesSchema.GetTable(dt.ServerId, dt.Database, dt.Schema, dt.Table);
        }

        public static TableSchema GetTable(this Cache cache, ITableIdentifier dt)
        {
            return cache.DatabasesSchema.GetTable(dt.ServerId, dt.Database, dt.Schema, dt.Table);
        }
    }
}
