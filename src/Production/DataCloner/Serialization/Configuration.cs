using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace DataCloner.Serialization
{
    [Serializable]
    public class Configuration
    {
        public static readonly string FileName = "dc.config";

        [XmlArrayItem("add")]
        public List<Connection> ConnectionStrings { get; set; }
        public StaticTable StaticTables { get; set; }
        public ManyToManyRelationshipsTable ManyToManyRelationshipsTable { get; set; }
        public DerivativeTableAccess DerivativeTableAccess { get; set; }

        public Configuration()
        {
            ConnectionStrings = new List<Connection>();            
        }

        public void Save()
        {
            var xs = new System.Xml.Serialization.XmlSerializer(this.GetType());
            var fs = new System.IO.FileStream(FileName, System.IO.FileMode.Create);
            var ns = new System.Xml.Serialization.XmlSerializerNamespaces();
            ns.Add("", "");

            xs.Serialize(fs, this, ns);
            fs.Close();
        }

        public static Configuration Load()
        {
            var xs = new System.Xml.Serialization.XmlSerializer(typeof(Configuration));
            if (System.IO.File.Exists(FileName))
            {
                var fs = new System.IO.FileStream(FileName, System.IO.FileMode.Open);
                var cReturn = (Configuration)xs.Deserialize(fs);
                fs.Close();
                return cReturn;
            }
            return new Configuration();            
        }
    }
}
