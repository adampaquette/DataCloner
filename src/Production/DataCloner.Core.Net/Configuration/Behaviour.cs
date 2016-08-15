using System;
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

        public Modifiers Modifiers { get; set; }

        public Behaviour()
        {
            Modifiers = new Modifiers();
        }
    }
}