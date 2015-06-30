namespace DataCloner.DataClasse.Cache
{
    internal static class DatabasesSchemaExtensions
    {
        public static TableSchema GetTable(this DatabasesSchema schema, IForeignKey fk)
        {
            return schema.GetTable(fk.ServerIdTo, fk.DatabaseTo, fk.SchemaTo, fk.TableTo);
        }

        public static TableSchema GetTable(this DatabasesSchema schema, IDerivativeTable dt)
        {
            return schema.GetTable(dt.ServerId, dt.Database, dt.Schema, dt.Table);
        }

        public static TableSchema GetTable(this DatabasesSchema schema, ITableIdentifier dt)
        {
            return schema.GetTable(dt.ServerId, dt.Database, dt.Schema, dt.Table);
        }
    }
}
