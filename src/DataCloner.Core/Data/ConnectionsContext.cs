using DataCloner.Core.Metadata.Context;
using System;
using System.Collections.Generic;

namespace DataCloner.Core.Data
{
    public class ConnectionsContext : IQueryProxy
    {
        private Dictionary<Int16, ConnectionContext> _ctx;

        public ConnectionContext this[SehemaIdentifier server] => _ctx[server.ServerId];

        public ConnectionContext this[Int16 server] =>_ctx[server];

        public void Init(Metadatas contextMetadata, IEnumerable<SqlConnection> connections)
        {
            _ctx = new Dictionary<short, ConnectionContext>();

            foreach (var conn in connections)
            {
                var providers = new ConnectionContext(
                    DbProviderFactories.GetFactory(conn.ProviderName).CreateConnection(),                    
                    MetadataProviderFactory.GetProvider(conn.ProviderName),
                    QueryProviderFactory.GetProvider(conn.ProviderName),
                    contextMetadata);
                _ctx.Add(conn.Id, providers);
            }
        }
    }
}