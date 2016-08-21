using DataCloner.Core.Metadata;
using DataCloner.Core.Metadata.Context;
using System;
using System.Collections.Generic;

namespace DataCloner.Core.Data
{
    public interface IQueryDispatcher
    {
        IQueryHelper this[SehemaIdentifier server] { get; }
        IQueryHelper this[Int16 server] { get; }
        IQueryHelper GetQueryHelper(SehemaIdentifier server);
        IQueryHelper GetQueryHelper(Int16 server);

        void InitProviders(Metadatas appMetadata, IEnumerable<SqlConnection> connections);
    }
}
