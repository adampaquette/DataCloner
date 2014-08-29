using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Security.Cryptography;

using DataCloner.Interface;
using DataCloner.DataClasse;
using DataCloner.DataClasse.Cache;
using DataCloner.DataClasse.Configuration;
using DataCloner.Framework;

using Murmur;

namespace DataCloner.DataAccess
{
    internal class QueryDispatcher : IQueryDispatcher
    {
        public Configuration Cache {get;set;}
        private Dictionary<Int16, IQueryProvider> _providers;

        public QueryDispatcher()
        {
        }

        public void Initialize(string cacheName = Configuration.CacheName)
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
            if (File.Exists(fullCacheName))
            {
                FileStream fsCache = null;
                BinaryReader brCache = null;

                try
                {
                    fsCache = new FileStream(fullCacheName, FileMode.Open);
                    brCache = new BinaryReader(fsCache);
                    
                    Cache.ConfigFileHash = brCache.ReadString();
                    cacheIsGood = Cache.ConfigFileHash == hashConfigFile;

                    if (cacheIsGood)
                    {
                        Configuration.DeserializeBody(brCache, Cache); //Load cache            
                        InitProviders(Cache.ConnectionStrings);
                        return;
                    }
                }
                finally
                { 
                    if (fsCache != null) fsCache.Close();
                }
            }

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
                    IQueryProvider provider = _providers[cs.Id];
                    string[] databases = provider.GetDatabasesName();
                    int nbDatabases = databases.Length;

                    for (int i = 0; i < nbDatabases; i++)
                    {
                        provider.GetColumns(Cache.CachedTables.LoadColumns, databases[i]);
                        provider.GetForeignKeys(Cache.CachedTables.LoadForeignKeys, databases[i]);
                    }
                }                
                Cache.CachedTables.FinalizeCache(config);

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
        private void InitProviders(List<Connection> conns)
        {
            _providers = new Dictionary<short, IQueryProvider>();

            foreach (Connection conn in conns)
            {
                Type t = Type.GetType(conn.ProviderName);
                var provider = FastActivator<string, Int16, Configuration>.CreateInstance(t, conn.ConnectionString, conn.Id, Cache) as IQueryProvider;
                _providers.Add(conn.Id, provider);
            }
        }

        public object[][] Select(IRowIdentifier ri)
        {
            return _providers[ri.ServerId].Select(ri);
        }

        public void Insert(ITableIdentifier ti, object[] row)
        {
            _providers[ti.ServerId].Insert(ti, row);
        }

        public void Update(IRowIdentifier ri, DataRow[] rows)
        {
            _providers[ri.ServerId].Update(ri, rows);
        }

        public void Delete(IRowIdentifier ri)
        {
            _providers[ri.ServerId].Delete(ri);
        }

        public object GetLastInsertedPk(Int16 serverId)
        {
            return _providers[serverId].GetLastInsertedPk();
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