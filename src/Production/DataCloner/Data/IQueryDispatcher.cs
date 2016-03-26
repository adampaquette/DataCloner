using DataCloner.Core.Metadata;
using System;
using System.Collections.Generic;

namespace DataCloner.Core.Data
{
    public interface IQueryDispatcher
    {
        IQueryHelper this[ServerIdentifier server] { get; }
        IQueryHelper this[Int16 server] { get; }
        IQueryHelper GetQueryHelper(ServerIdentifier server);
        IQueryHelper GetQueryHelper(Int16 server);

        void InitProviders(AppMetadata appMetadata, IEnumerable<SqlConnection> connections);
    }
}
