using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace DataCloner.DataClasse.Configuration
{
    [Serializable]
    public class Application
    {
		[XmlAttribute]
		public Int16 Id { get; set; }
		[XmlAttribute]
        public string Name { get; set; }
        [XmlArrayItem("Add")]
        public List<Connection> ConnectionStrings { get; set; }
        public ModifiersTemplates ModifiersTemplates { get; set; }
        public List<ClonerBehaviour> ClonerBehaviours { get; set; }
        public List<Map> Maps { get; set; }

        public Application()
        {
            ConnectionStrings = new List<Connection>();
            ModifiersTemplates = new ModifiersTemplates();
            ClonerBehaviours = new List<ClonerBehaviour>();
            Maps = new List<Map>();
        }
    }
}
