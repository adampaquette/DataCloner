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

        public void InitProviders(Cache cache)
        {
            _queryHelpers = new Dictionary<short, IQueryHelper>();

            foreach (var conn in cache.ConnectionStrings)
                _queryHelpers.Add(conn.Id, QueryHelperFactory.GetQueryHelper(cache.DatabasesSchema, conn.ProviderName, conn.ConnectionString));
        }
    }
}