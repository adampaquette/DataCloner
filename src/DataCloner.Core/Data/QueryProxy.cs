using DataCloner.Core.Metadata.Context;
using System;
using System.Collections.Generic;

namespace DataCloner.Core.Data
{
    public class QueryProxy : IQueryProxy
    {
        public QueryProxy()
        {
            Contexts = new Dictionary<short, ConnectionContext>();
        }

        public Dictionary<Int16, ConnectionContext> Contexts { get; }

        public ConnectionContext this[SehemaIdentifier server] => Contexts[server.ServerId];

        public ConnectionContext this[Int16 server] =>Contexts[server];

        public void Init( IEnumerable<SqlConnection> connections, Metadatas contextMetadata)
        {
            Contexts.Clear();

            foreach (var conn in connections)
            {
                var providers = new ConnectionContext(
                    DbProviderFactories.GetFactory(conn.ProviderName).CreateConnection(),                    
                    MetadataProviderFactory.GetProvider(conn.ProviderName),
                    QueryProviderFactory.GetProvider(conn.ProviderName),
                    contextMetadata);
                Contexts.Add(conn.Id, providers);
            }
        }
    }
}