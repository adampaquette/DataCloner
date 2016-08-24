using System;
using System.Diagnostics;
using System.Xml.Serialization;

namespace DataCloner.Core.Configuration
{
    [DebuggerDisplay("{Name}")]
    [Serializable]
    public class DataBuilder : IEquatable<DataBuilder>
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public string BuilderName { get; set; }

        public override bool Equals(object obj)
        {
            var o = obj as DataBuilder;
            return Equals(o);
        }

        public bool Equals(DataBuilder other)
        {
            return other != null && other.Name == Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
