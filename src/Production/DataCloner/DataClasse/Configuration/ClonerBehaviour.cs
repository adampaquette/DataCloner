using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace DataCloner.DataClasse.Configuration
{
    [Serializable]
    public class ClonerBehaviour
    {
        [XmlAttribute]
        public Int16 Id { get; set; }

        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public string Description { get; set; }

        public Modifiers Modifiers { get; set; }

        public ClonerBehaviour()
        {
            Modifiers = new Modifiers();
        }
    }
}