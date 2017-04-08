using System;
using System.Diagnostics;
using System.Xml.Serialization;

namespace DataCloner.Core.Configuration
{
    [DebuggerDisplay("{Name}")]
    [Serializable]
    public class ForeignKeyRemoveColumn : IEquatable<ForeignKeyRemoveColumn>
    {
        [XmlAttribute]
        public string Name { get; set; }

        public override bool Equals(object obj)
        {
            var o = obj as ForeignKeyRemoveColumn;
            return Equals(o);
        }

        public bool Equals(ForeignKeyRemoveColumn other)
        {
            return other != null && other.Name == Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
