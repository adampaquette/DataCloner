using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace DataCloner.Configuration
{
    [Serializable]
    public class Map
    {
        [XmlAttribute]
        public int Id { get; set; }
        [XmlAttribute]
        public string From { get; set; }
        [XmlAttribute]
        public string To { get; set; }
        [XmlAttribute]
        public string UsableBehaviours { get; set; }
        [XmlArrayItem("Var")]
        public List<Variable> Variables { get; set; }
        [XmlElement("Road")]
        public List<Road> Roads { get; set; }

        public Map()
        {
            Variables = new List<Variable>();
            Roads = new List<Road>();
        }
    }

    [Serializable]
    public class Road
    {
        [XmlAttribute]
        public string ServerSrc { get; set; }
        [XmlAttribute]
        public string DatabaseSrc { get; set; }
        [XmlAttribute]
        public string SchemaSrc { get; set; }
        [XmlAttribute]
        public string ServerDst { get; set; }
        [XmlAttribute]
        public string DatabaseDst { get; set; }
        [XmlAttribute]
        public string SchemaDst { get; set; }
    }

    [Serializable]
    public class Variable
    {
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string Value { get; set; }
    }
}