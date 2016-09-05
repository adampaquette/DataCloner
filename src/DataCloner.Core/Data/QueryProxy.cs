using DataCloner.Core.Metadata.Context;
using System.Collections.Generic;

namespace DataCloner.Core.Data
{
    public class QueryProxy : IQueryProxy
    {
        public QueryProxy()
        {
            Contexts = new Dictionary<short, ConnectionContext>();
        }

        public Dictionary<short, ConnectionContext> Contexts { get; }

        public ConnectionContext this[SehemaIdentifier server] => Contexts[server.ServerId];

        public ConnectionContext this[short server] =>Contexts[server];

        public void Init( IEnumerable<SqlConnection> connections, Metadatas contextMetadata)
        {
            Contexts.Clear();

            foreach (var conn in connections)
            {
                var context = new ConnectionContext(
                    DbProviderFactories.GetFactory(conn.ProviderName).CreateConnection(),                    
                    MetadataProviderFactory.GetProvider(conn.ProviderName),
                    QueryProviderFactory.GetProvider(conn.ProviderName),
                    contextMetadata);
                Contexts.Add(conn.Id, context);
            }
        }
    }
}