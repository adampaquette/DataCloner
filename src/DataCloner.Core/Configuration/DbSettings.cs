using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;

namespace DataCloner.Core.Configuration
{
    [DebuggerDisplay("{Id.ToString() + \"_\" + Var}")]
    [Serializable]
    public class DbSettings : IEquatable<DbSettings>
    {
        [XmlAttribute]
        public short Id { get; set; }

        [XmlAttribute]
        public string Var { get; set; }

        [XmlAttribute]
        public string Description { get; set; }

        [XmlAttribute]
        public short BasedOn { get; set; }

        [XmlElement("Table")]
        public List<Table> Tables { get; set; }

        public DbSettings()
        {
            Tables = new List<Table>();
        }

        public override bool Equals(object obj)
        {
            var o = obj as DbSettings;
            return Equals(o);
        }

        public bool Equals(DbSettings other)
        {
            return other != null && other.Id == Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
