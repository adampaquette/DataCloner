using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DataCloner.Core.Configuration
{
    [Serializable]
    public class Server : IEquatable<Server>
    {
        [XmlAttribute]
        public string Var { get; set; }
        [XmlAttribute]
        public string Description { get; set; }
        [XmlAttribute]
        public Int16 TemplateId { get; set; }
        [XmlAttribute]
        public Int16 BasedOn { get; set; }
        [XmlElement("Database")]
        public List<Database> Databases { get; set; }

        public Server()
        {
            Databases = new List<Database>();
        }

        public override bool Equals(object obj)
        {
            var o = obj as Server;
            return Equals(o);
        }

        public bool Equals(Server other)
        {
            return other != null && other.Var == Var;
        }

        public override int GetHashCode()
        {
            return Var.GetHashCode();
        }
    }
}
