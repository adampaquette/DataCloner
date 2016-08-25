using DataCloner.Core.Metadata.Context;
using System;
using System.Collections.Generic;

namespace DataCloner.Core.Data
{
    public class QueryDispatcher : IQueryDispatcher
    {
        private Dictionary<Int16, ServerContext> _queryHelpers;

        public ServerContext this[SehemaIdentifier server] => _queryHelpers[server.ServerId];

        public ServerContext this[Int16 server] =>_queryHelpers[server];

        public void InitProviders(Metadatas contextMetadata, IEnumerable<SqlConnection> connections)
        {
            _queryHelpers = new Dictionary<short, ServerContext>();

            foreach (var conn in connections)
            {
                var providers = new ServerContext(
                    DbProviderFactories.GetFactory(conn.ProviderName).CreateConnection(),                    
                    MetadataProviderFactory.GetProvider(conn.ProviderName),
                    QueryProviderFactory.GetProvider(conn.ProviderName),
                    contextMetadata);
                _queryHelpers.Add(conn.Id, providers);
            }
        }
    }
}