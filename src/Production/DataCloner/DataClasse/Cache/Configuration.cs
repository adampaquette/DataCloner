using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;

using DataCloner.DataAccess;
using DataCloner.DataClasse.Configuration;

using Murmur;

namespace DataCloner.DataClasse.Cache
{
    internal class Configuration
    {
        public const string CacheName = "dc";
        public const string Extension = ".cache";

        public string ConfigFileHash { get; set; }
        public List<Connection> ConnectionStrings { get; set; }
        public CachedTables CachedTables { get; set; }

        public Configuration()
        {
            ConnectionStrings = new List<Connection>();
            CachedTables = new CachedTables();
        }        

        public void Serialize(Stream stream)
        {
            Serialize(new BinaryWriter(stream));
        }

        public static Configuration Deserialize(Stream stream)
        {
            return Deserialize(new BinaryReader(stream));
        }

        public void Serialize(BinaryWriter stream)
        {
            stream.Write(ConfigFileHash);
            stream.Write(ConnectionStrings.Count);
            foreach (var cs in ConnectionStrings)
                cs.Serialize(stream);
            CachedTables.Serialize(stream);
            
            stream.Flush();
        }

        public static Configuration Deserialize(BinaryReader stream)
        {
            var config = new Configuration();
            config.ConfigFileHash = stream.ReadString();

            Configuration.DeserializeBody(stream, config);

            return config;
        }

        public static Configuration DeserializeBody(BinaryReader stream, Configuration config)
        {
            int nbConnection = stream.ReadInt32();
            for (int i = 0; i < nbConnection; i++)
                config.ConnectionStrings.Add(Connection.Deserialize(stream));

            config.CachedTables = CachedTables.Deserialize(stream);

            return config;
        }
    }
}
