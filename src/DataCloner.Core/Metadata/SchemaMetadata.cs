using System.Collections.Generic;

namespace DataCloner.Core.Metadata
{
    /// <summary>
    /// Contains all the metadatas about a SQL server's schema.
    /// </summary>
    /// <example>Tables, Columns, PrimaryKeys, ForeignKeys...</example>
    public sealed class SchemaMetadata : HashSet<TableMetadata> { }
}
