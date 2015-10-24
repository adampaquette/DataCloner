using System;
using System.Collections.Generic;
using System.Data;
using DataCloner.Internal;
using DataCloner.Metadata;

namespace DataCloner.Data
{
    public class QueryDispatcher : IQueryDispatcher
    {
        private Dictionary<Int16, IQueryHelper> _queryHelpers;

        public IQueryHelper this[IServerIdentifier server]
        {
            get { return _queryHelpers[server.ServerId]; }
        }

        public IQueryHelper this[Int16 server]
        {
            get { return _queryHelpers[server]; }
        }

        public IQueryHelper GetQueryHelper(IServerIdentifier server)
        {
            return _queryHelpers[server.ServerId];
        }

        public IQueryHelper GetQueryHelper(Int16 server)
        {
            return _queryHelpers[server];
        }

        public void InitProviders(MetadataContainer container)
        {
            _queryHelpers = new Dictionary<short, IQueryHelper>();

            foreach (var conn in container.ConnectionStrings)
                _queryHelpers.Add(conn.Id, QueryHelperFactory.GetQueryHelper(container.Metadatas, conn.ProviderName, conn.ConnectionString));
        }
    }
}