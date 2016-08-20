using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace DataCloner.Core.Configuration
{
    [DataContract]
    public class Behaviour
    {
        [DataMember]
        public Int16 Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Description { get; set; }

        public Modifiers Modifiers { get; set; }

        public Behaviour()
        {
            Modifiers = new Modifiers();
        }
    }
}