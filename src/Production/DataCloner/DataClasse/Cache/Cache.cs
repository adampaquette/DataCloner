using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Security.Cryptography;

using DataCloner.DataClasse;
using DataCloner.DataClasse.Cache;
using DataCloner.DataClasse.Configuration;

using Murmur;
using System.Runtime.Serialization.Formatters.Binary;
using DataCloner.DataAccess;

namespace DataCloner.DataClasse.Cache
{
    internal class Cache
    {
        public string ConfigFileHash { get; set; }
        public List<Connection> ConnectionStrings { get; set; }
        public DatabasesSchema DatabasesSchema { get; set; }
        public static Cache Current { get; set; }

        public Cache()
        {
            ConnectionStrings = new List<Connection>();
            DatabasesSchema = new DatabasesSchema();
        }        

        public void Serialize(Stream stream)
        {
            Serialize(new BinaryWriter(stream));
        }

        public static Cache Deserialize(Stream stream)
        {
            return Deserialize(new BinaryReader(stream));
        }

        public void Serialize(BinaryWriter stream)
        {
            stream.Write(ConfigFileHash);
            stream.Write(ConnectionStrings.Count);
            foreach (var cs in ConnectionStrings)
                cs.Serialize(stream);
            DatabasesSchema.Serialize(stream);
            
            stream.Flush();
        }

        public static Cache Deserialize(BinaryReader stream)
        {
            var config = new Cache();
            config.ConfigFileHash = stream.ReadString();

            Cache.DeserializeBody(stream, config);

            return config;
        }

        public static Cache DeserializeBody(BinaryReader stream, Cache config)
        {
            int nbConnection = stream.ReadInt32();
            for (int i = 0; i < nbConnection; i++)
                config.ConnectionStrings.Add(Connection.Deserialize(stream));

            config.DatabasesSchema = DatabasesSchema.Deserialize(stream);

            return config;
        }

        public static Cache Load(string cacheFile, string configHash)
        {
            Cache cache = null;
            bool cacheIsGood = false;

            //Check if cached file match with config file version
            if (File.Exists(cacheFile))
            {
                using (var fsCache = new FileStream(cacheFile, FileMode.Open))
                using (var brCache = new BinaryReader(fsCache))
                {
                    var fileHash = brCache.ReadString();
                    cacheIsGood = fileHash == configHash;

                    if (cacheIsGood)
                    {
                        cache = new Cache();
                        cache.ConfigFileHash = fileHash;
                        Cache.DeserializeBody(brCache, cache); //Load cache            
                    }
                }
            }
            return cache;
        }

        public void Save(string path)
        {
            var fsCache = new FileStream(path, FileMode.Create);
            this.Serialize(fsCache);
            fsCache.Close();
        }

        public static void InitCache(string application, string mapFrom, string mapTo, int? configId)
        {
            if (String.IsNullOrWhiteSpace(mapFrom)) throw new ArgumentNullException(nameof(mapFrom));
            if (String.IsNullOrWhiteSpace(mapTo)) throw new ArgumentNullException(nameof(mapTo));
            if (!File.Exists(Configuration.Configuration.CONFIG_FILE_NAME))
                throw new FileNotFoundException("The configuration file doesn't exist!");

            string cacheFileName = mapFrom + "_" + mapTo;
            if (configId != null)
                cacheFileName += "_" + configId.ToString();
            cacheFileName += ".cache";

            //Hash the selected map and the cloner configuration to see if it match the lasted builded cache
            var config = Configuration.Configuration.Load(Configuration.Configuration.CONFIG_FILE_NAME);
            var app = config?.Applications.Where(a => a.Name == application).FirstOrDefault();
            if (app == null)
                throw new KeyNotFoundException(String.Format("There is no configuration for the application name '{0}'.", application));

            var map = app.Maps.Where(m => m.From == mapFrom && m.To == mapTo).FirstOrDefault();
            if (map == null)
                throw new KeyNotFoundException(String.Format(
                    "There is no map where attribute From='{0}' and To='{1}' in the configuration for the application name '{2}'.",
                    mapFrom, mapTo, application));

            //Get binary view 
            var configData = new MemoryStream();
            var bf = new BinaryFormatter();
            bf.Serialize(configData, map);

            ClonerConfiguration clonerConfig = null;

            if (map.UsableConfigs.Split(',').ToList().Contains(configId.ToString()))
            {
                clonerConfig = app.ClonerConfigurations.Where(c => c.Id == configId).FirstOrDefault();
                if (clonerConfig == null)
                    throw new KeyNotFoundException(String.Format(
                        "There is no cloner configuration '{0}' in the configuration for the application name '{1}'.",
                        clonerConfig, application));

                bf.Serialize(configData, clonerConfig);
            }

            //Hash user config
            HashAlgorithm murmur = MurmurHash.Create32(managed: false);
            string configHash = Encoding.Default.GetString(murmur.ComputeHash(configData));

            var cache = Cache.Load(cacheFileName, configHash);
            if (cache != null)
            {
                QueryDispatcher.InitProviders(cache.ConnectionStrings);
            }
            else
            {
                //Rebuild cache
                cache = new Cache();
                cache.ConfigFileHash = configHash;

                //Copy connection strings
                foreach (var cs in app.ConnectionStrings)
                    cache.ConnectionStrings.Add(new DataClasse.Cache.Connection(cs.Id, cs.ProviderName, cs.ConnectionString));

                QueryDispatcher.InitProviders(cache.ConnectionStrings);

                //Start fetching each server
                foreach (var cs in app.ConnectionStrings)
                {
                    IQueryHelper provider = QueryDispatcher.GetQueryHelper(cs.Id);

                    foreach (var database in provider.GetDatabasesName())
                    {
                        provider.GetColumns(cache.DatabasesSchema.LoadColumns, database);
                        provider.GetForeignKeys(cache.DatabasesSchema.LoadForeignKeys, database);
                        provider.GetUniqueKeys(cache.DatabasesSchema.LoadUniqueKeys, database);
                    }
                }

                if (clonerConfig != null)
                    cache.DatabasesSchema.FinalizeCache(clonerConfig);

                //Save cache
                cache.Save(cacheFileName);
            }
            Current = cache;
        }
    }
}