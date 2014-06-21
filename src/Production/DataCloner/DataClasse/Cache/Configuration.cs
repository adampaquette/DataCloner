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
            StaticTable st = new StaticTable();
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
            
            HashAlgorithm murmur128 = MurmurHash.Create128(managed: false); 
            byte[] file = File.ReadAllBytes(ConfigurationXml.FileName);           
            byte[] hash = murmur128.ComputeHash(file);
        }

        public void Serialize(Stream stream)
        {
            BinaryWriter bw = new BinaryWriter(stream);
        }

        public static Connection Deserialize(Stream stream)
        {
            BinaryReader br = new BinaryReader(stream);
            return null;
        }
    }
}
