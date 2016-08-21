using System;
using System.Collections.Generic;

namespace DataCloner.Core.Metadata
{
    /// <summary>
    /// Contains all the metadatas about a SQL server's database.
    /// </summary>
    /// <example>Schemas, Tables, Columns, PrimaryKeys, ForeignKeys...</example>
    public sealed class DatabaseMetadata : Dictionary<string, SchemaMetadata>
    {
        public DatabaseMetadata() : base(StringComparer.OrdinalIgnoreCase) { }
    }
}
