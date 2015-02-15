using System;
using System.Collections.Generic;
using System.Data;
using DataCloner.DataClasse;
using DataCloner.DataClasse.Cache;

namespace DataCloner.DataAccess
{
    internal class QueryDispatcher : IQueryDispatcher
    {
        private Dictionary<Int16, IQueryHelper> _providers;

        public IQueryHelper this[IServerIdentifier server] => _providers[server.ServerId];
        public IQueryHelper this[Int16 server] => _providers[server];
        public IDbConnection GetConnection(IServerIdentifier server) => _providers[server.ServerId].Connection;
        public IDbConnection GetConnection(Int16 server) => _providers[server].Connection;
        public IQueryHelper GetQueryHelper(IServerIdentifier server) => _providers[server.ServerId];
        public IQueryHelper GetQueryHelper(Int16 server) => _providers[server];

        public void InitProviders(Cache cache)
        {
            _providers = new Dictionary<short, IQueryHelper>();

            foreach (var conn in cache.ConnectionStrings)
                _providers.Add(conn.Id, QueryHelperFactory.GetQueryHelper(cache, conn.ProviderName, conn.ConnectionString, conn.Id));
        }

        public void Dispose()
        {
            //TODO : IDISPOSABLE
            foreach (var conn in _providers)
            {
                conn.Value.Dispose();
            }
        }
    }
}