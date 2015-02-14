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
    internal static class QueryDispatcher
    {
        private static Dictionary<Int16, IQueryHelper> _providers = null;

        public static IDbConnection GetConnection(IServerIdentifier server)
        {
            return _providers[server.ServerId].Connection;
        }

        public static IDbConnection GetConnection(Int16 server)
        {
            return _providers[server].Connection;
        }

        public static IQueryHelper GetQueryHelper(IServerIdentifier server)
        {
            return _providers[server.ServerId];
        }

        public static IQueryHelper GetQueryHelper(Int16 server)
        {
            return _providers[server];
        }

        public static void InitProviders(List<DataClasse.Cache.Connection> conns)
        {
            _providers = new Dictionary<short, IQueryHelper>();

            foreach (DataClasse.Cache.Connection conn in conns)
                _providers.Add(conn.Id, QueryHelperFactory.GetQueryHelper(conn.ProviderName, conn.ConnectionString, conn.Id));
        }

        public static void Dispose()
        {
            //TODO : IDISPOSABLE
            foreach (var conn in _providers)
            {
                conn.Value.Dispose();
            }
        }
    }

    internal static class QueryDispatcherExtensions
    {
        public static IDbConnection GetConnection(this IServerIdentifier server)
        {
            return QueryDispatcher.GetQueryHelper(server).Connection;
        }

        public static IQueryHelper GetQueryHelper(this IServerIdentifier server)
        {
            return QueryDispatcher.GetQueryHelper(server);
        }
    }
}