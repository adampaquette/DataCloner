using DataCloner.Internal;

namespace DataCloner.Metadata
{
    internal static class AppMetadataExtensions
    {
        public static TableMetadata GetTable(this AppMetadata schema, ForeignKey fk)
        {
            return schema.GetTable(fk.ServerIdTo, fk.DatabaseTo, fk.SchemaTo, fk.TableTo);
        }

        public static TableMetadata GetTable(this AppMetadata schema, DerivativeTable dt)
        {
            return schema.GetTable(dt.ServerId, dt.Database, dt.Schema, dt.Table);
        }

        public static TableMetadata GetTable(this AppMetadata schema, TableIdentifier dt)
        {
            return schema.GetTable(dt.ServerId, dt.Database, dt.Schema, dt.Table);
        }
    }
}
