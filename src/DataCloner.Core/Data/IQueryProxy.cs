using DataCloner.Core.Metadata.Context;
using System;
using System.Collections.Generic;

namespace DataCloner.Core.Data
{
    public interface IQueryProxy
    {
        ConnectionContext this[SehemaIdentifier server] { get; }

        ConnectionContext this[Int16 server] { get; }

        void Init(Metadatas contextMetadata, IEnumerable<SqlConnection> connections);
    }
}
