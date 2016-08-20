using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace DataCloner.Core.Configuration
{
    [Serializable]
    public class MapFrom
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public string UsableBehaviours { get; set; }

        [XmlElement("Var")]
        public List<Variable> Variables { get; set; }

        [XmlElement("To")]
        public List<MapTo> MapTos { get; set; }

        [XmlElement("Road")]
        public List<Road> Roads { get; set; }

        public MapFrom()
        {
            Variables = new List<Variable>();
            Roads = new List<Road>();
            MapTos = new List<MapTo>();
        }
    }
}