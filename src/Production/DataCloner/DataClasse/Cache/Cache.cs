using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using DataCloner.DataAccess;
using DataCloner.DataClasse.Configuration;
using Murmur;

namespace DataCloner.DataClasse.Cache
{
    internal sealed class Cache
    {
        public string ConfigFileHash { get; set; }
        public Dictionary<ServerIdentifier, ServerIdentifier> ServerMap { get; set; }
        public List<Connection> ConnectionStrings { get; set; }
        public DatabasesSchema DatabasesSchema { get; set; }

        public Cache()
        {
            ServerMap = new Dictionary<ServerIdentifier, ServerIdentifier>();
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
            stream.Write(ServerMap.Count);
            foreach (var sm in ServerMap)
            {
                sm.Key.Serialize(stream);
                sm.Value.Serialize(stream);
            }
            stream.Write(ConnectionStrings.Count);
            foreach (var cs in ConnectionStrings)
                cs.Serialize(stream);
            DatabasesSchema.Serialize(stream);

            stream.Flush();
        }

        public static Cache Deserialize(BinaryReader stream)
        {
            var config = new Cache { ConfigFileHash = stream.ReadString() };

            DeserializeBody(stream, config);

            return config;
        }

        public static Cache DeserializeBody(BinaryReader stream, Cache config)
        {
            var nbServerMap = stream.ReadInt32();
            for (var i = 0; i < nbServerMap; i++)
            {
                var src = ServerIdentifier.Deserialize(stream);
                var dst = ServerIdentifier.Deserialize(stream);
                config.ServerMap.Add(src, dst);
            }
            var nbConnection = stream.ReadInt32();
            for (var i = 0; i < nbConnection; i++)
                config.ConnectionStrings.Add(Connection.Deserialize(stream));

            config.DatabasesSchema = DatabasesSchema.Deserialize(stream);

            return config;
        }

        public static Cache Load(string cacheFile, string configHash)
        {
            Cache cache = null;

            //Check if cached file match with config file version
            if (File.Exists(cacheFile))
            {
                using (var fsCache = new FileStream(cacheFile, FileMode.Open))
                using (var brCache = new BinaryReader(fsCache))
                {
                    var fileHash = brCache.ReadString();

                    if (fileHash == configHash)
                    {
                        cache = new Cache { ConfigFileHash = fileHash };
                        DeserializeBody(brCache, cache);
                    }
                }
            }
            return cache;
        }

        public void Save(string path)
        {
            var fsCache = new FileStream(path, FileMode.Create);
            Serialize(fsCache);
            fsCache.Close();
        }

        internal delegate Cache CacheInitialiser(
            IQueryDispatcher dispatcher, Configuration.Configuration config, string appName, int mapId, int? configId);
        public static Cache Init(IQueryDispatcher dispatcher, Configuration.Configuration config, 
                                 string appName, int mapId, int? configId)
        {
            if (dispatcher == null) throw new ArgumentNullException(nameof(dispatcher));
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (String.IsNullOrWhiteSpace(appName)) throw new ArgumentNullException(nameof(appName));
           
            ClonerConfiguration clonerConfig = null;

            var appConfig = config.Applications.FirstOrDefault(a => a.Name == appName);
            if(appConfig == null) throw new Exception(String.Format("Application '{0}' not found in configuration file!", appName));

            var map = appConfig.Maps.FirstOrDefault(m => m.Id == mapId);
            if (map == null) throw new Exception(String.Format("Map id '{0}' not found in configuration file for application '{1}'!", mapId, appName));

            var cacheFileName = appName + " _" + map.From + "-" + map.To;
            if (configId != null)
                cacheFileName += "_" + configId;
            cacheFileName += ".cache";

            //Hash the selected map and the cloner configuration to see if it match the lasted builded cache
            var app = config.Applications.FirstOrDefault(a => a.Name == appName);
            if (app == null)
                throw new KeyNotFoundException(String.Format("There is no configuration for the appName name '{0}'.", appName));

            //Get binary view 
            var configData = new MemoryStream();
            var bf = new BinaryFormatter();
            bf.Serialize(configData, map);

            if (map.UsableConfigs != null && map.UsableConfigs.Split(',').ToList().Contains(configId.ToString()))
            {
                clonerConfig = app.ClonerConfigurations.FirstOrDefault(c => c.Id == configId);
                if (clonerConfig == null)
                    throw new KeyNotFoundException(String.Format(
                        "There is no cloner configuration '{0}' in the configuration for the appName name '{1}'.",
                        configId, appName));

                bf.Serialize(configData, clonerConfig);
            }
            configData.Position = 0;

            //Hash user config
            HashAlgorithm murmur = MurmurHash.Create32(managed: false);
            var configHash = Encoding.Default.GetString(murmur.ComputeHash(configData));

            var cache = Load(cacheFileName, configHash);
            if (cache != null)
                dispatcher.InitProviders(cache);
            else
                cache = BuildCache(dispatcher, clonerConfig, cacheFileName, app, map, configHash);
            return cache;
        }

        private static Cache BuildCache(IQueryDispatcher dispatcher, ClonerConfiguration clonerConfig, string cacheFileName, Application app, Map map, string configHash)
        {
            var cache = new Cache { ConfigFileHash = configHash, ServerMap = map };

            //Copy connection strings
            foreach (var cs in app.ConnectionStrings)
                cache.ConnectionStrings.Add(new Connection(cs.Id, cs.ProviderName, cs.ConnectionString));

            dispatcher.InitProviders(cache);

            //Start fetching each server
            foreach (var cs in app.ConnectionStrings)
            {
                var provider = dispatcher.GetQueryHelper(cs.Id);

                foreach (var database in provider.GetDatabasesName())
                {
                    provider.GetColumns(cache.DatabasesSchema.LoadColumns, database);
                    provider.GetForeignKeys(cache.DatabasesSchema.LoadForeignKeys, database);
                    provider.GetUniqueKeys(cache.DatabasesSchema.LoadUniqueKeys, database);
                }
            }
            cache.DatabasesSchema.FinalizeCache(clonerConfig);

            //Save cache
            cache.Save(cacheFileName);
            return cache;
        }
    }
}