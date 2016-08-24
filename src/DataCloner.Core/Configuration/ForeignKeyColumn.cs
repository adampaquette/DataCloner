using System;
using System.Diagnostics;
using System.Xml.Serialization;

namespace DataCloner.Core.Configuration
{
    [DebuggerDisplay("{NameFrom + \"_\" + NameTo}")]
    [Serializable]
    public class ForeignKeyColumn : IEquatable<ForeignKeyColumn>
    {
        [XmlAttribute]
        public string NameFrom { get; set; }

        [XmlAttribute]
        public string NameTo { get; set; }

        public override bool Equals(object obj)
        {
            var o = obj as ForeignKeyColumn;
            return Equals(o);
        }

        public bool Equals(ForeignKeyColumn other)
        {
            return other != null &&
                other.NameFrom == NameFrom &&
                other.NameTo == NameTo;
        }

        public override int GetHashCode()
        {
            return (NameFrom+NameTo).GetHashCode();
        }
    }
}
