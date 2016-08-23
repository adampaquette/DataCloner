using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace DataCloner.Core.Configuration
{

    [Serializable]
    public class Table : IEquatable<Table>
    {
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public NullableBool IsStatic { get; set; }
        public DerivativeTableGlobal DerativeTableGlobal { get; set; }
        [XmlArrayItem("Column")]
        public List<DataBuilder> DataBuilders { get; set; }
        public ForeignKeys ForeignKeys { get; set; }

        public Table()
        {
            DataBuilders = new List<DataBuilder>();
            DerativeTableGlobal = new DerivativeTableGlobal();
            ForeignKeys = new ForeignKeys();
        }

        public override bool Equals(object obj)
        {
            var o = obj as Table;
            return Equals(o);
        }

        public bool Equals(Table other)
        {
            return other != null && other.Name == Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
