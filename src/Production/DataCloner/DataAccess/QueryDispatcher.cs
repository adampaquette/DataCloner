using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Security.Cryptography;

using DataCloner.DataClasse;
using DataCloner.DataClasse.Cache;
using DataCloner.DataClasse.Configuration;

using Murmur;

namespace DataCloner.DataAccess
{
    internal class QueryDispatcher
    {
        private Dictionary<Int16, IQueryHelper> _providers = null;

        public IDbConnection GetConnection(IServerIdentifier server)
        {
            return _providers[server.ServerId].Connection;
        }

        public IDbConnection GetConnection(Int16 server)
        {
            return _providers[server].Connection;
        }

        public IQueryHelper GetQueryHelper(IServerIdentifier server)
        {
            return _providers[server.ServerId];
        }

        public IQueryHelper GetQueryHelper(Int16 server)
        {
            return _providers[server];
        }

        public void InitProviders(Cache cache)
        {
            _providers = new Dictionary<short, IQueryHelper>();

            foreach (DataClasse.Cache.Connection conn in cache.ConnectionStrings)
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