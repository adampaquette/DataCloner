using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Security.Cryptography;

using DataCloner.DataClasse;
using DataCloner.DataClasse.Cache;
using DataCloner.DataClasse.Configuration;
using DataCloner.Framework;

using Murmur;

namespace DataCloner.DataAccess
{
    internal static class QueryDispatcher
    {
        public static Configuration Cache { get; set; }
        private static Dictionary<Int16, IQueryHelper> _providers;

        public static void Initialize(string cacheName = Configuration.CacheName)
        {
            string fullCacheName = cacheName + Configuration.Extension;
            string fullConfigName = cacheName + ConfigurationXml.Extension;
            bool cacheIsGood = false;

            Cache = new Configuration();

            if (!File.Exists(fullConfigName))
                throw new FileNotFoundException("The configuration file doesn't exist!");

            //Hash user config
            HashAlgorithm murmur = MurmurHash.Create32(managed: false);
            byte[] configFile = File.ReadAllBytes(fullConfigName);
            string hashConfigFile = Encoding.Default.GetString(murmur.ComputeHash(configFile));

            //Check if cached file match with config file version
            //if (File.Exists(fullCacheName))
            //{
            //    using (var fsCache = new FileStream(fullCacheName, FileMode.Open))
            //    using (var brCache = new BinaryReader(fsCache))
            //    {
            //        Cache.ConfigFileHash = brCache.ReadString();
            //        cacheIsGood = Cache.ConfigFileHash == hashConfigFile;

            //        if (cacheIsGood)
            //        {
            //            Configuration.DeserializeBody(brCache, Cache); //Load cache            
            //            InitProviders(Cache.ConnectionStrings);
            //            return;
            //        }
            //    }
            //}

            //Rebuild cache
            if (!cacheIsGood)
            {
                var config = ConfigurationXml.Load(fullConfigName);
                Cache.ConfigFileHash = hashConfigFile;

                //Copy connection strings
                foreach (var cs in config.ConnectionStrings)
                    Cache.ConnectionStrings.Add(new Connection(cs.Id, cs.ProviderName, cs.ConnectionString, cs.SameConfigAsId));

                InitProviders(Cache.ConnectionStrings);

                //Start fetching each server
                foreach (var cs in config.ConnectionStrings)
                {
                    IQueryHelper provider = _providers[cs.Id];

                    foreach (var database in provider.GetDatabasesName())
                    {
                        provider.GetColumns(Cache.CachedTablesSchema.LoadColumns, database);
                        provider.GetForeignKeys(Cache.CachedTablesSchema.LoadForeignKeys, database);
                    }
                }
                Cache.CachedTablesSchema.FinalizeCache(config);

                //Save cache
                var fsCache = new FileStream(fullCacheName, FileMode.Create);
                Cache.Serialize(fsCache);
                fsCache.Close();
            }
        }

        /// <summary>
        /// Récupération des providers qui seront utilisés pour effectuer les requêtes
        /// </summary>
        /// <param name="config"></param>
        private static void InitProviders(List<Connection> conns)
        {
            _providers = new Dictionary<short, IQueryHelper>();

            foreach (Connection conn in conns)
                _providers.Add(conn.Id, QueryHelperFactory.GetQueryHelper(conn.ProviderName, conn.ConnectionString, conn.Id, Cache));
        }

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

        public static void CreateDatabaseFromCache(ServerIdentifier source, ServerIdentifier destination)
        {

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