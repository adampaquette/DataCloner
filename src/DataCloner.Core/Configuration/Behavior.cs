using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;

namespace DataCloner.Core.Configuration
{
    [DebuggerDisplay("{Id.ToString() + \"_\" + Name}")]
    [Serializable]
    public class Behavior
    {
        [XmlAttribute]
        public Int16 Id { get; set; }

        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public string Description { get; set; }

        [XmlElement("DbSetting")]
        public List<DbSettings> DbSettings { get; set; }

        public Behavior()
        {
            DbSettings = new List<DbSettings>();
        }
    }
}