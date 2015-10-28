using DataCloner.Internal;

namespace DataCloner.Metadata
{
    internal static class AppMetadataExtensions
    {
        public static TableMetadata GetTable(this AppMetadata schema, IForeignKey fk)
        {
            return schema.GetTable(fk.ServerIdTo, fk.DatabaseTo, fk.SchemaTo, fk.TableTo);
        }

        public static TableMetadata GetTable(this AppMetadata schema, IDerivativeTable dt)
        {
            return schema.GetTable(dt.ServerId, dt.Database, dt.Schema, dt.Table);
        }

        public static TableMetadata GetTable(this AppMetadata schema, ITableIdentifier dt)
        {
            return schema.GetTable(dt.ServerId, dt.Database, dt.Schema, dt.Table);
        }
    }
}
