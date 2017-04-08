using System;
using System.Collections.Generic;

namespace DataCloner.Core.Metadata
{
    /// <summary>
    /// Contains all the metadatas of a SQL server.
    /// </summary>
    /// <example>Databases, Schemas, Tables, Columns, PrimaryKeys, ForeignKeys...</example>
    public sealed class ServerMetadata : Dictionary<string, DatabaseMetadata>
    {
        public ServerMetadata() : base(StringComparer.OrdinalIgnoreCase) { }
    }
}
