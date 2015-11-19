using DataCloner.Internal;
using DataCloner.Metadata;
using System;

namespace DataCloner.Data
{
    public interface IQueryDispatcher
    {
        IQueryHelper this[ServerIdentifier server] { get; }
        IQueryHelper this[Int16 server] { get; }
        IQueryHelper GetQueryHelper(ServerIdentifier server);
        IQueryHelper GetQueryHelper(Int16 server);

        void InitProviders(MetadataContainer container);
    }
}
