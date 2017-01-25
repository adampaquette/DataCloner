using DataCloner.Core.Configuration;
using DataCloner.Core.Metadata.Context;
using System.Collections.Generic;

namespace DataCloner.Core.Data
{
    public interface IQueryProxy
    {
        Dictionary<short, ConnectionsContext> Contexts { get; }

        ConnectionsContext this[SehemaIdentifier server] { get; }

        ConnectionsContext this[short server] { get; }

        void Init(IEnumerable<Connection> connections, Metadatas contextMetadata);
    }
}
