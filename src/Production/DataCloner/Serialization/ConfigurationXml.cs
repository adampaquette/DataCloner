using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace DataCloner.Serialization
{
    [Serializable]
    [XmlRoot("Configuration")]
    public class ConfigurationXml
    {
        public static readonly string FileName = "dc.config";

        [XmlArrayItem("add")]
        public List<ConnectionXml> ConnectionStrings { get; set; }
        public StaticTableXml StaticTables { get; set; }
        public DerivativeTableAccessXml DerivativeTableAccess { get; set; }
        public ForeignKeysXml ForeignKeys { get; set; }
        public DataBuilderXml DataBuilders { get; set; }

        public ConfigurationXml()
        {
            ConnectionStrings = new List<ConnectionXml>();
            StaticTables = new StaticTableXml();
            DerivativeTableAccess = new DerivativeTableAccessXml();
            ForeignKeys = new ForeignKeysXml();
            DataBuilders = new DataBuilderXml();
        }

        public void Save()
        {
            var xs = new XmlSerializer(GetType());
            var fs = new FileStream(FileName, FileMode.Create);
            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            xs.Serialize(fs, this, ns);
            fs.Close();
        }

        public static ConfigurationXml Load()
        {
            var xs = new XmlSerializer(typeof(ConfigurationXml));
            if (!File.Exists(FileName)) return new ConfigurationXml();
            var fs = new FileStream(FileName, FileMode.Open);
            var cReturn = (ConfigurationXml)xs.Deserialize(fs);
            fs.Close();
            return cReturn;
        }
    }
}
