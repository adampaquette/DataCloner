using DataCloner.DataAccess;
using DataCloner.DataClasse.Configuration;
using DataCloner.Framework;
using Murmur;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;

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

        public static void Initialize(IQueryDispatcher dispatcher, Application app, int mapId, int? behaviourId, ref Cache cache)
        {
            if (dispatcher == null) throw new ArgumentNullException("dispatcher");
            if (app == null) throw new ArgumentNullException("app");
            if (app.ConnectionStrings != null && !app.ConnectionStrings.Any()) throw new NullReferenceException("ConnectionStrings");

            var map = app.Maps.FirstOrDefault(m => m.Id == mapId);
            if (map == null)
                throw new Exception(string.Format("Map id '{0}' not found in configuration file for application '{1}'!", mapId, app.Name));

            var cacheFileName = app.Name + "_" + map.From + "-" + map.To;
            if (behaviourId != null)
                cacheFileName += "_" + behaviourId;
            cacheFileName += ".cache";

            //Hash the selected map, connnectionStrings and the cloner 
            //configuration to see if it match the lasted builded cache
            var configData = new MemoryStream();
            var bf = new BinaryFormatter();
            bf.Serialize(configData, map);
            bf.Serialize(configData, app.ConnectionStrings);

            ClonerBehaviour clonerBehaviour = null;
            if (map.UsableBehaviours != null && map.UsableBehaviours.Split(',').ToList().Contains(behaviourId.ToString()))
            {
                clonerBehaviour = app.ClonerBehaviours.FirstOrDefault(c => c.Id == behaviourId);
                if (clonerBehaviour == null)
                    throw new KeyNotFoundException(
                        string.Format("There is no behaviour '{0}' in the configuration for the appName name '{1}'.", behaviourId, app.Name));

                bf.Serialize(configData, clonerBehaviour);
                bf.Serialize(configData, app.ModifiersTemplates);
            }
            configData.Position = 0;

            //Hash user config
            HashAlgorithm murmur = MurmurHash.Create32(managed: false);
            var configHash = Encoding.Default.GetString(murmur.ComputeHash(configData));

            //If in-memory cache is good, we use it
            if (cache != null && cache.ConfigFileHash == configHash)
                return;
            //If cache on disk is good, we use it
            cache = Load(cacheFileName, configHash);
            if (cache != null)
                dispatcher.InitProviders(cache);
            //We rebuild the cache
            else
                cache = BuildCache(dispatcher, cacheFileName, app, clonerBehaviour, map, configHash);
        }

        public static void InitializeSchema(IQueryDispatcher dispatcher, Application app, ref Cache cache)
        {
            if (dispatcher == null) throw new ArgumentNullException("dispatcher");
            if (app == null) throw new ArgumentNullException("app");
            if (app.ConnectionStrings != null && !app.ConnectionStrings.Any()) throw new NullReferenceException("ConnectionStrings");

            var cacheFileName = app.Name + ".schema";

            //Hash the connnectionStrings to see if it match the last build
            var configData = new MemoryStream();
            var bf = new BinaryFormatter();
            bf.Serialize(configData, app.ConnectionStrings);
            configData.Position = 0;

            HashAlgorithm murmur = MurmurHash.Create32(managed: false);
            var configHash = Encoding.Default.GetString(murmur.ComputeHash(configData));

            //If in-memory cache is good, we use it
            if (cache != null && cache.ConfigFileHash == configHash)
                return;
            //If cache on disk is good, we use it
            cache = Load(cacheFileName, configHash);
            if (cache != null)
                dispatcher.InitProviders(cache);
            //We rebuild the cache
            else
                cache = BuildSchema(dispatcher, cacheFileName, app.ConnectionStrings, configHash);
        }

        /// <summary>
        /// Also merge user configs
        /// </summary>
        /// <param name="dispatcher"></param>
        /// <param name="cacheFileName"></param>
        /// <param name="app"></param>
        /// <param name="clonerBehaviour"></param>
        /// <param name="map"></param>
        /// <param name="configHash"></param>
        /// <returns></returns>
        private static Cache BuildCache(IQueryDispatcher dispatcher, string cacheFileName,
                                        Application app, ClonerBehaviour clonerBehaviour,
                                        Map map, string configHash)
        {
            var cache = new Cache { ConfigFileHash = configHash, ServerMap = map.ConvertToDictionnary() };

            //Get servers source
            var serversSource = map.Roads.Select(r => r.ServerSrc).Distinct().ToList();

            //Replace variables
            for (int i = 0; i < serversSource.Count(); i++)
            {
                if (serversSource[i].IsVariable())
                {
                    var configVar = map.Variables.FirstOrDefault(v => v.Name == serversSource[i]);
                    if (configVar == null)
                        throw new Exception(string.Format("The variable '{0}' is not found in the map with id='{1}'.", serversSource[i], map.Id));
                    serversSource[i] = configVar.Value.ToString();
                }
            }

            //Copy connection strings
            foreach (var cs in app.ConnectionStrings.Where(c => serversSource.Contains(c.Id.ToString())))
                cache.ConnectionStrings.Add(new Connection(cs.Id, cs.ProviderName, cs.ConnectionString));

            if (cache.ConnectionStrings.Count() == 0)
                throw new Exception("No connectionStrings!");

            FetchSchema(dispatcher, ref cache);
            var behaviour = clonerBehaviour.Build(app.ModifiersTemplates, map.Variables);
            cache.DatabasesSchema.FinalizeCache(behaviour);

            //Save cache
            cache.Save(cacheFileName);
            return cache;
        }

        /// <summary>
        /// Only build the database schema.
        /// </summary>
        /// <param name="dispatcher"></param>
        /// <param name="cacheFileName"></param>
        /// <param name="connections"></param>
        /// <param name="configHash"></param>
        /// <returns></returns>
        private static Cache BuildSchema(IQueryDispatcher dispatcher, string cacheFileName,
                                         List<Configuration.Connection> connections, string configHash)
        {
            var cache = new Cache { ConfigFileHash = configHash };

            //Copy connection strings
            foreach (var cs in connections)
                cache.ConnectionStrings.Add(new Connection(cs.Id, cs.ProviderName, cs.ConnectionString));

            FetchSchema(dispatcher, ref cache);
            cache.DatabasesSchema.FinalizeSchema();

            //Save cache
            cache.Save(cacheFileName);
            return cache;
        }

        private static void FetchSchema(IQueryDispatcher dispatcher, ref Cache cache)
        {
            dispatcher.InitProviders(cache);

            foreach (var cs in cache.ConnectionStrings)
            {
                var provider = dispatcher.GetQueryHelper(cs.Id);

                foreach (var database in provider.GetDatabasesName())
                {
                    provider.GetColumns(cache.DatabasesSchema.LoadColumns, cs.Id, database);
                    provider.GetForeignKeys(cache.DatabasesSchema.LoadForeignKeys, cs.Id, database);
                    provider.GetUniqueKeys(cache.DatabasesSchema.LoadUniqueKeys, cs.Id, database);
                }
            }
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