using DataCloner.Core.Metadata;
using DataCloner.Core.Metadata.Context;
using System;
using System.Collections.Generic;

namespace DataCloner.Core.Data
{
    public class QueryDispatcher : IQueryDispatcher
    {
        private Dictionary<Int16, IQueryHelper> _queryHelpers;
        public IQueryHelper this[SehemaIdentifier server] => _queryHelpers[server.ServerId];
        public IQueryHelper this[Int16 server] =>_queryHelpers[server]; 
        public IQueryHelper GetQueryHelper(SehemaIdentifier server) => _queryHelpers[server.ServerId];
        public IQueryHelper GetQueryHelper(Int16 server) => _queryHelpers[server];

        public void InitProviders(Metadatas contextMetadata, IEnumerable<SqlConnection> connections)
        {
            _queryHelpers = new Dictionary<short, IQueryHelper>();

            foreach (var conn in connections)
                _queryHelpers.Add(conn.Id, QueryHelperFactory.GetQueryHelper(contextMetadata, conn.ProviderName, conn.ConnectionString));
        }
    }
}