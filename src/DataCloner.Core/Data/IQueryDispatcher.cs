using DataCloner.Core.Metadata.Context;
using System;
using System.Collections.Generic;

namespace DataCloner.Core.Data
{
    public interface IQueryDispatcher
    {
        ServerContext this[SehemaIdentifier server] { get; }

        ServerContext this[Int16 server] { get; }

        void InitProviders(Metadatas appMetadata, IEnumerable<SqlConnection> connections);
    }
}
