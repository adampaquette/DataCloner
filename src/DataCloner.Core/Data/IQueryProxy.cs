using DataCloner.Core.Metadata.Context;
using System;
using System.Collections.Generic;

namespace DataCloner.Core.Data
{
    public interface IQueryProxy
    {
        Dictionary<Int16, ConnectionContext> Contexts { get; }

        ConnectionContext this[SehemaIdentifier server] { get; }

        ConnectionContext this[Int16 server] { get; }

        void Init(IEnumerable<SqlConnection> connections, Metadatas contextMetadata);
    }
}
