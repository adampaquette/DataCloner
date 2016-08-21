namespace DataCloner.Core.Metadata
{
    internal static class ExecutionContextMetadataExtensions
    {
        public static TableMetadata GetTable(this ExecutionContextMetadata schema, ForeignKey fk)
        {
            return schema.GetTable(fk.ServerIdTo, fk.DatabaseTo, fk.SchemaTo, fk.TableTo);
        }

        public static TableMetadata GetTable(this ExecutionContextMetadata schema, DerivativeTable dt)
        {
            return schema.GetTable(dt.ServerId, dt.Database, dt.Schema, dt.Table);
        }

        public static TableMetadata GetTable(this ExecutionContextMetadata schema, TableIdentifier dt)
        {
            return schema.GetTable(dt.ServerId, dt.Database, dt.Schema, dt.Table);
        }
    }
}
