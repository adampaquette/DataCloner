using System;
using System.Diagnostics;
using System.Xml.Serialization;

namespace DataCloner.Core.Configuration
{
    [DebuggerDisplay("{Id}")]
    [Serializable]
    public class SchemaVar : IEquatable<SchemaVar>
    {
        [XmlAttribute]
        public string Id { get; set; }

        [XmlAttribute]
        public string Server { get; set; }

        [XmlAttribute]
        public string Database { get; set; }

        [XmlAttribute]
        public string Schema { get; set; }

        public override bool Equals(object obj)
        {
            var o = obj as SchemaVar;
            return Equals(o);
        }

        public bool Equals(SchemaVar other)
        {
            return other != null && other.Id == Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
