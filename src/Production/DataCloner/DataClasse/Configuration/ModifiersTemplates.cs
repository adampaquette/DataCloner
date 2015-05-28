using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace DataCloner.DataClasse.Configuration
{
    [Serializable]
    public class ModifiersTemplates
    {
        [XmlElement("ServerModifier")]
        public List<ServerModifier> ServerModifiers { get; set; }
        [XmlElement("DatabaseModifier")]
        public List<DatabaseModifier> DatabaseModifiers { get; set; }
        [XmlElement("SchemaModifier")]
        public List<SchemaModifier> SchemaModifiers { get; set; }

        public ModifiersTemplates()
        {
            ServerModifiers = new List<ServerModifier>();
            DatabaseModifiers = new List<DatabaseModifier>();
            SchemaModifiers = new List<SchemaModifier>();
        }
    }
}