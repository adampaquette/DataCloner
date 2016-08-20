using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace DataCloner.Core.Configuration
{
    [Serializable]
    public class Schema : IEquatable<Schema>
    {
        [XmlAttribute]
        public string Var { get; set; }
        [XmlAttribute]
        public string Description { get; set; }
        [XmlAttribute]
        public Int16 TemplateId { get; set; }
        [XmlAttribute]
        public Int16 BasedOn { get; set; }
        [XmlElement("Table")]
        public List<Table> Tables { get; set; }

        public Schema()
        {
            Tables = new List<Table>();
        }

        public override bool Equals(object obj)
        {
            var o = obj as Schema;
            return Equals(o);
        }

        public bool Equals(Schema other)
        {
            return other != null && other.Var == Var;
        }

        public override int GetHashCode()
        {
            return Var.GetHashCode();
        }
    }
}
