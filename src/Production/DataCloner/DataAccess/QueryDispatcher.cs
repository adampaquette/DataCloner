using System;
using System.Collections.Generic;
using System.Data;
using DataCloner.DataClasse;
using DataCloner.DataClasse.Cache;

namespace DataCloner.DataAccess
{
    public class QueryDispatcher : IQueryDispatcher
    {
        private Dictionary<Int16, IQueryHelper> _queryHelpers;

        public IQueryHelper this[IServerIdentifier server] => _queryHelpers[server.ServerId];

        public IQueryHelper this[Int16 server] => _queryHelpers[server];
        public IDbConnection GetConnection(IServerIdentifier server) => _queryHelpers[server.ServerId].Connection;
        public IDbConnection GetConnection(Int16 server) => _queryHelpers[server].Connection;
        public IQueryHelper GetQueryHelper(IServerIdentifier server) => _queryHelpers[server.ServerId];
        public IQueryHelper GetQueryHelper(Int16 server) => _queryHelpers[server];

        public void InitProviders(Cache cache)
        {
            _queryHelpers = new Dictionary<short, IQueryHelper>();

            foreach (var conn in cache.ConnectionStrings)
                _queryHelpers.Add(conn.Id, QueryHelperFactory.GetQueryHelper(cache.DatabasesSchema, conn.ProviderName, conn.ConnectionString, conn.Id));
        }
    }
}