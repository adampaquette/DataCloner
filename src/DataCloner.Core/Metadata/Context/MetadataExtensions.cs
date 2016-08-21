namespace DataCloner.Core.Metadata.Context
{
    internal static class MetadataExtensions
    {
        public static TableMetadata GetTable(this Metadatas schema, ForeignKey fk)
        {
            return schema.GetTable(fk.ServerIdTo, fk.DatabaseTo, fk.SchemaTo, fk.TableTo);
        }

        public static TableMetadata GetTable(this Metadatas schema, DerivativeTable dt)
        {
            return schema.GetTable(dt.ServerId, dt.Database, dt.Schema, dt.Table);
        }

        public static TableMetadata GetTable(this Metadatas schema, TableIdentifier dt)
        {
            return schema.GetTable(dt.ServerId, dt.Database, dt.Schema, dt.Table);
        }
    }
}
