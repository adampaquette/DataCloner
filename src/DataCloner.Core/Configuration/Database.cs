using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace DataCloner.Core.Configuration
{
    [Serializable]
    public class Database : IEquatable<Database>
    {
        [XmlAttribute]
        public string Var { get; set; }
        [XmlAttribute]
        public string Description { get; set; }
        [XmlAttribute]
        public Int16 TemplateId { get; set; }
        [XmlAttribute]
        public Int16 BasedOn { get; set; }
        [XmlElement("Schema")]
        public List<Schema> Schemas { get; set; }

        public Database()
        {
            Schemas = new List<Schema>();
        }

        public override bool Equals(object obj)
        {
            var o = obj as Database;
            return Equals(o);
        }

        public bool Equals(Database other)
        {
            return other != null && other.Var == Var;
        }

        public override int GetHashCode()
        {
            return Var.GetHashCode();
        }
    }
}
