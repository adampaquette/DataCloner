using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DataCloner.DataClasse.Configuration
{
    class Configuration
    {
        public string ConfigFileHash { get; set; }
        public List<Connection> ConnectionStrings { get; set; }
        public DerivativeTable DerivativeTables { get; set; }
        public StaticTable StaticTables { get; set; }

        public Configuration()
        {

            StaticTable st = new StaticTable();
           // st.Add()
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
