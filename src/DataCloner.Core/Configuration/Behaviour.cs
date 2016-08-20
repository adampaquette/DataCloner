using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace DataCloner.Core.Configuration
{
    [Serializable]
    public class Behaviour
    {
        [XmlAttribute]
        public Int16 Id { get; set; }

        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public string Description { get; set; }

        [XmlElement("DbSetting")]
        public List<DbSettings> DbSettings { get; set; }

        public Behaviour()
        {
            DbSettings = new List<DbSettings>();
        }
    }
}