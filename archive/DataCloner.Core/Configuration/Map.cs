using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DataCloner.Core.Configuration
{
    [DataContract]
    public class Map
    {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public string From { get; set; }
        [DataMember]
        public string To { get; set; }
        [DataMember]
        public string UsableBehaviours { get; set; }
        //TODO : [XmlArrayItem("Var")]
        public List<Variable> Variables { get; set; }
        //TODO : [XmlElement("Road")]
        public List<Road> Roads { get; set; }

        public Map()
        {
            Variables = new List<Variable>();
            Roads = new List<Road>();
        }
    }

    [DataContract]
    public class Road
    {
        [DataMember]
        public string ServerSrc { get; set; }
        [DataMember]
        public string DatabaseSrc { get; set; }
        [DataMember]
        public string SchemaSrc { get; set; }
        [DataMember]
        public string ServerDst { get; set; }
        [DataMember]
        public string DatabaseDst { get; set; }
        [DataMember]
        public string SchemaDst { get; set; }
    }

    [DataContract]
    public class Variable
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Value { get; set; }
    }
}