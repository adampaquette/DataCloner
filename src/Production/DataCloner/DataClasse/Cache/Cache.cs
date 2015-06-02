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
    public sealed class Cache
    {
        internal delegate void CacheInitialiser(IQueryDispatcher dispatcher, Application app, int mapId, int? configId, ref Cache cache);

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

        public static void Init(IQueryDispatcher dispatcher, Application app, int mapId, int? configId, ref Cache cache)
        {
            if (dispatcher == null) throw new ArgumentNullException(nameof(dispatcher));
            if (app == null) throw new ArgumentNullException(nameof(app));
            if (!app.ConnectionStrings?.Any() ?? false) throw new NullReferenceException("ConnectionStrings");

            var map = app.Maps.FirstOrDefault(m => m.Id == mapId);
            if (map == null)
                throw new Exception($"Map id '{mapId}' not found in configuration file for application '{app.Name}'!");

            var cacheFileName = app.Name + " _" + map.From + "-" + map.To;
            if (configId != null)
                cacheFileName += "_" + configId;
            cacheFileName += ".cache";

            //Hash the selected map, connnectionStrings and the cloner 
            //configuration to see if it match the lasted builded cache
            var configData = new MemoryStream();
            var bf = new BinaryFormatter();
            bf.Serialize(configData, map);
            bf.Serialize(configData, app.ConnectionStrings);

            ClonerConfiguration clonerConfig = null;
            if (map.UsableConfigs != null && map.UsableConfigs.Split(',').ToList().Contains(configId.ToString()))
            {
                clonerConfig = app.ClonerConfigurations.FirstOrDefault(c => c.Id == configId);
                if (clonerConfig == null)
                    throw new KeyNotFoundException(
                        $"There is no cloner configuration '{configId}' in the configuration for the appName name '{app.Name}'.");

                bf.Serialize(configData, clonerConfig);
                bf.Serialize(configData, app.ModifiersTemplates);
            }
            configData.Position = 0;

            //Hash user config
            HashAlgorithm murmur = MurmurHash.Create32(managed: false);
            var configHash = Encoding.Default.GetString(murmur.ComputeHash(configData));

            //If in-memory cache is good, we use it
            if (cache?.ConfigFileHash == configHash)
                return;
            //If cache on disk is good, we use it
            cache = Load(cacheFileName, configHash);
            if (cache != null)
                dispatcher.InitProviders(cache);
            //We rebuild the cache
            else
                cache = BuildCache(dispatcher, cacheFileName, app, clonerConfig, map, configHash);
        }

        private static Cache BuildCache(IQueryDispatcher dispatcher, string cacheFileName,
                                        Application app, ClonerConfiguration clonerConfig,
                                        Map map, string configHash)
        {
            var cache = new Cache { ConfigFileHash = configHash, ServerMap = map };

            //Get servers source
            var serversSource = map.Roads.Select(r => r.ServerSrc).Distinct();

            //Copy connection strings
            foreach (var cs in app.ConnectionStrings.Where(c=>serversSource.Contains(c.Id.ToString())))
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
            clonerConfig.Build(app.ModifiersTemplates, map.Variables);
            cache.DatabasesSchema.FinalizeCache(clonerConfig);

            //Save cache
            cache.Save(cacheFileName);
            return cache;
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

            if (File.Exists(cacheFile))
            {
                using (var fsCache = new FileStream(cacheFile, FileMode.Open))
                using (var brCache = new BinaryReader(fsCache))
                {
                    var fileHash = brCache.ReadString();

                    //Check if cached file match with config file version
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
    }
}