using System.Collections.Generic;
using System.Linq;
using DataCloner.Core.Metadata.Context;
using DataCloner.Core.Configuration;
using DataCloner.Core.Data;

namespace DataCloner.Core
{
    public class ConnectionsContext
    {
        public Dictionary<Connection, ConnectionContext> Connections { get; internal set; }

        public Metadatas Metadatas { get; internal set; }

        public ConnectionContext this[SehemaIdentifier schema] => this[schema.ServerId];

        public ConnectionContext this[string server]
        {
            get
            {
                //TODO: UPTIMISER ÇA
                var key = Connections.Keys.First(c => c.Id == server);
                return Connections[key];
            }
        }

        public ConnectionsContext()
        {
            Connections = new Dictionary<Connection, ConnectionContext>();
        }

        public void Initialize(List<Connection> connectionsString, Metadatas metadatas)
        {
            Connections.Clear();

            foreach (var conn in connectionsString)
            {
                var dbConnection = DbProviderFactories.GetFactory(conn.ProviderName).CreateConnection();
                dbConnection.ConnectionString = conn.ConnectionString;

                Connections.Add(conn,
                    new ConnectionContext(dbConnection,
                    QueryProviderFactory.GetProvider(conn.ProviderName)));
            }

            Metadatas = metadatas;
        }
    }
}
