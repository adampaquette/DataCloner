namespace DataCloner.Core.Metadata.Context
{
    internal static class MetadataExtensions
    {
        public static TableMetadata GetTable(this Metadatas metadatas, ForeignKey fk)
        {
            return metadatas.GetTable(fk.ServerIdTo, fk.DatabaseTo, fk.SchemaTo, fk.TableTo);
        }

        public static TableMetadata GetTable(this Metadatas metadatas, DerivativeTable dt)
        {
            return metadatas.GetTable(dt.ServerId, dt.Database, dt.Schema, dt.Table);
        }

        public static TableMetadata GetTable(this Metadatas metadatas, TableIdentifier dt)
        {
            return metadatas.GetTable(dt.ServerId, dt.Database, dt.Schema, dt.Table);
        }
    }
}
