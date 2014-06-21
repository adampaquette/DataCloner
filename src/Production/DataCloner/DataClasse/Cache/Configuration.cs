using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;

using DataCloner.DataClasse.Configuration;

using Murmur;

namespace DataCloner.DataClasse.Cache
{
    class Configuration
    {
        public static readonly string FileName = "db.cache";

        public string ConfigFileHash { get; set; }
        public List<Connection> ConnectionStrings { get; set; }
        public DerivativeTable DerivativeTables { get; set; }
        public StaticTable StaticTables { get; set; }

        public Configuration()
        {
            ConnectionStrings = new List<Connection>();
            DerivativeTables = new DerivativeTable();
            StaticTables = new StaticTable();
        }

        public void Initialize()
        { 
            /*
            Si le fichier de cache est présent et que le hash du fichier de config est identique
                Charger la cache
            Sinon
                Si le fichier de config est présent
                    Charger le fichier de config
                    Construire la cache
                Sinon
                    Erreur
            */
            var fsCache = new FileStream(FileName, FileMode.Open);
            var brCache = new BinaryReader(fsCache);

            //Check if cached file match with config file version
            HashAlgorithm murmur = MurmurHash.Create32(managed: false); 
            byte[] configFile = File.ReadAllBytes(ConfigurationXml.FileName);           
            string hashConfigFile = Encoding.Default.GetString(murmur.ComputeHash(configFile));

            ConfigFileHash = brCache.ReadString();

            //If no match, reload cache
            if (ConfigFileHash != hashConfigFile)
            {
                //RELOAD CACHE
            }
            else
            { 
                //Load cache
                Configuration.DeserializeBody(brCache, this);
            }
            brCache.Close();
            fsCache.Close();
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
            DerivativeTables.Serialize(stream);
            StaticTables.Serialize(stream);
            
            stream.Flush();
        }

        public static Configuration Deserialize(BinaryReader stream)
        {
            var config = new Configuration();
            config.ConfigFileHash = stream.ReadString();

            Configuration.DeserializeBody(stream, config);

            return config;
        }

        private static Configuration DeserializeBody(BinaryReader stream, Configuration config)
        {
            int nbConnection = stream.ReadInt32();
            for (int i = 0; i < nbConnection; i++)
                config.ConnectionStrings.Add(Connection.Deserialize(stream));

            config.DerivativeTables = DerivativeTable.Deserialize(stream);
            config.StaticTables = StaticTable.Deserialize(stream);

            return config;
        }
    }
}
