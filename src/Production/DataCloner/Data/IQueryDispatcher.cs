using DataCloner.Internal;
using DataCloner.Metadata;
using System;

namespace DataCloner.Data
{
    public interface IQueryDispatcher
    {
        IQueryHelper this[IServerIdentifier server] { get; }
        IQueryHelper this[Int16 server] { get; }
        IQueryHelper GetQueryHelper(IServerIdentifier server);
        IQueryHelper GetQueryHelper(Int16 server);

        void InitProviders(MetadataContainer container);
    }
}
