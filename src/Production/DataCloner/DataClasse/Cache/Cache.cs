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

namespace DataCloner.DataClasse.Cache
{
    internal class Cache
    {
        public const string CacheName = "dc";
        public const string Extension = ".cache";

        public string ConfigFileHash { get; set; }
        public List<Connection> ConnectionStrings { get; set; }
        public DatabasesSchema DatabasesSchema { get; set; }

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
    }
}