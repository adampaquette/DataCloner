using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace DataCloner.Core.Configuration
{
    [Serializable]
    public class DbSettings : IEquatable<DbSettings>
    {
        [XmlAttribute]
        public Int16 Id { get; set; }

        [XmlAttribute]
        public string Var { get; set; }

        [XmlAttribute]
        public string Description { get; set; }

        [XmlAttribute]
        public Int16 BasedOn { get; set; }

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
