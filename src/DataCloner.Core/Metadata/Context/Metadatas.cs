using DataCloner.Core.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DataCloner.Core.Metadata.Context
{
    /// <summary>
    /// Contains metadatas grouped by connection string. 
    /// </summary>
    /// <remarks>ServerId / Database / Schema -> TableMetadata[]</remarks>
    public sealed class Metadatas : Dictionary<string, ServerMetadata>
    {
        public TableMetadata GetTable(string server, string database, string schema, string table)
        {
            if (ContainsKey(server) &&
                this[server].ContainsKey(database) &&
                this[server][database].ContainsKey(schema))
                return this[server][database][schema].FirstOrDefault(t => t.Name.Equals(table, StringComparison.OrdinalIgnoreCase));
            return null;
        }

        public SchemaMetadata this[string server, string database, string schema]
        {
            get
            {
                if (ContainsKey(server) &&
                    this[server].ContainsKey(database) &&
                    this[server][database].ContainsKey(schema))
                {
                    return this[server][database][schema];
                }
                return null;
            }
            set
            {
                if (!ContainsKey(server))
                    Add(server, new ServerMetadata());

                if (!this[server].ContainsKey(database))
                    this[server].Add(database, new DatabaseMetadata());

                if (!this[server][database].ContainsKey(schema))
                    this[server][database].Add(schema, value);
                else
                    this[server][database][schema] = value;
            }
        }

        public void Serialize(Stream stream, FastAccessList<object> referenceTracking = null)
        {
            Serialize(new BinaryWriter(stream, Encoding.UTF8, true), referenceTracking);
        }

        public static Metadatas Deserialize(Stream stream, FastAccessList<object> referenceTracking = null)
        {
            return Deserialize(new BinaryReader(stream, Encoding.UTF8, true), referenceTracking);
        }

        public void Serialize(BinaryWriter output, FastAccessList<object> referenceTracking = null)
        {
            output.Write(Count);
            foreach (var server in this)
            {
                output.Write(server.Key);
                output.Write(server.Value.Count);
                foreach (var database in server.Value)
                {
                    output.Write(database.Key);
                    output.Write(database.Value.Count);
                    foreach (var schema in database.Value)
                    {
                        var nbRows = schema.Value.Count;
                        output.Write(schema.Key);
                        output.Write(nbRows);
                        foreach (var table in schema.Value)
                        {
                            if (referenceTracking == null)
                                table.Serialize(output);
                            else
                            {
                                var id = referenceTracking.TryAdd(table);
                                output.Write(id);
                            }
                        }
                    }
                }
            }
        }

        public static Metadatas Deserialize(BinaryReader stream, FastAccessList<object> referenceTracking = null)
        {
            var cTables = new Metadatas();

            var nbServers = stream.ReadInt32();
            for (var n = 0; n < nbServers; n++)
            {
                var serverId = stream.ReadString();
                cTables.Add(serverId, new ServerMetadata());

                var nbDatabases = stream.ReadInt32();
                for (var j = 0; j < nbDatabases; j++)
                {
                    var database = stream.ReadString();
                    cTables[serverId].Add(database, new DatabaseMetadata());

                    var nbSchemas = stream.ReadInt32();
                    for (var k = 0; k < nbSchemas; k++)
                    {
                        var schemaMetadata = new SchemaMetadata();
                        var schema = stream.ReadString();

                        var nbTablesFrom = stream.ReadInt32();
                        for (var l = 0; l < nbTablesFrom; l++)
                        {
                            TableMetadata table;

                            if (referenceTracking == null)
                                table = TableMetadata.Deserialize(stream);
                            else
                            {
                                var id = stream.ReadInt32();
                                table = (TableMetadata)referenceTracking[id];
                            }
                            schemaMetadata.Add(table);
                        }

                        cTables[serverId][database].Add(schema, schemaMetadata);
                    }
                }
            }
            return cTables;
        }
    }
}