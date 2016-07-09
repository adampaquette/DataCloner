using DataCloner.Core.Metadata;
using System;
using System.Collections.Generic;

namespace DataCloner.Core.Data
{
    public class QueryDispatcher : IQueryDispatcher
    {
        private Dictionary<Int16, IQueryHelper> _queryHelpers;
        public IQueryHelper this[ServerIdentifier server] => _queryHelpers[server.ServerId];
        public IQueryHelper this[Int16 server] =>_queryHelpers[server]; 
        public IQueryHelper GetQueryHelper(ServerIdentifier server) => _queryHelpers[server.ServerId];
        public IQueryHelper GetQueryHelper(Int16 server) => _queryHelpers[server];

        public void InitProviders(AppMetadata appMetadata, IEnumerable<SqlConnection> connections)
        {
            _queryHelpers = new Dictionary<short, IQueryHelper>();

            foreach (var conn in connections)
                _queryHelpers.Add(conn.Id, QueryHelperFactory.GetQueryHelper(appMetadata, conn.ProviderName, conn.ConnectionString));
        }
    }
}