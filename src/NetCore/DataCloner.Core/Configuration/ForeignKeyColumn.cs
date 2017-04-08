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
        public string Source { get; set; }

        [XmlAttribute]
        public string Destination { get; set; }

        public override bool Equals(object obj)
        {
            var o = obj as ForeignKeyColumn;
            return Equals(o);
        }

        public bool Equals(ForeignKeyColumn other)
        {
            return other != null &&
                other.Source == Source &&
                other.Destination == Destination;
        }

        public override int GetHashCode()
        {
            return (Source+Destination).GetHashCode();
        }
    }
}
