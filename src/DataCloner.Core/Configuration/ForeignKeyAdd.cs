using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;

namespace DataCloner.Core.Configuration
{
    [DebuggerDisplay("{DestinationVar + \"_\" + TableTo}")]
    [Serializable]
    public class ForeignKeyAdd : IEquatable<ForeignKeyAdd>
    {
        [XmlAttribute]
        public string DestinationSchema { get; set; }

        [XmlAttribute]
        public string DestinationTable { get; set; }

        [XmlElement("Column")]
        public List<ForeignKeyColumn> Columns { get; set; }

        public ForeignKeyAdd()
        {
            Columns = new List<ForeignKeyColumn>();
        }

        public override bool Equals(object obj)
        {
            var o = obj as ForeignKeyAdd;
            return Equals(o);
        }

        public bool Equals(ForeignKeyAdd other)
        {
            if (other == null)
                return false;

            if (other.DestinationSchema == DestinationSchema &&
                other.DestinationTable == DestinationTable)
            {
                foreach (var col in Columns)
                {
                    if (!other.Columns.Contains(col))
                        return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            return (DestinationSchema+DestinationTable).GetHashCode();
        }
    }
}
