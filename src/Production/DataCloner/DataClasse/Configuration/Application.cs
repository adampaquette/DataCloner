using System;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace DataCloner.DataClasse.Configuration
{
    public class Application
    {
        [XmlAttribute]
        public string Name { get; set; }
        [XmlArrayItem("add")]
        public List<Connection> ConnectionStrings { get; set; }
        public List<ClonerConfiguration> ClonerConfigurations { get; set; }
        public List<Map> Maps { get; set; }

        public Application()
        {
            ConnectionStrings = new List<Connection>();
            ClonerConfigurations = new List<ClonerConfiguration>();
            Maps = new List<Map>();
        }
    }
}
