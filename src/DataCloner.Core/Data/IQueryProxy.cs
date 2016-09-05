using DataCloner.Core.Metadata.Context;
using System.Collections.Generic;

namespace DataCloner.Core.Data
{
    public interface IQueryProxy
    {
        Dictionary<short, ConnectionContext> Contexts { get; }

        ConnectionContext this[SehemaIdentifier server] { get; }

        ConnectionContext this[short server] { get; }

        void Init(IEnumerable<SqlConnection> connections, Metadatas contextMetadata);
    }
}
