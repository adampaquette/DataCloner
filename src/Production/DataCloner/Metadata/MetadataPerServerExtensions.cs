using DataCloner.Internal;

namespace DataCloner.Metadata
{
    internal static class MetadataPerServerExtensions
    {
        public static TableMetadata GetTable(this MetadataPerServer schema, IForeignKey fk)
        {
            return schema.GetTable(fk.ServerIdTo, fk.DatabaseTo, fk.SchemaTo, fk.TableTo);
        }

        public static TableMetadata GetTable(this MetadataPerServer schema, IDerivativeTable dt)
        {
            return schema.GetTable(dt.ServerId, dt.Database, dt.Schema, dt.Table);
        }

        public static TableMetadata GetTable(this MetadataPerServer schema, ITableIdentifier dt)
        {
            return schema.GetTable(dt.ServerId, dt.Database, dt.Schema, dt.Table);
        }
    }
}
