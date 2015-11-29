using System;
using System.Diagnostics;
using System.IO;

namespace DataCloner
{
    [DebuggerDisplay("{ServerId.ToString() + \".\" + Database + \".\" + Schema + \".\" + Table}")]
    public class TableIdentifier : ServerIdentifier, IEquatable<TableIdentifier>
    {
        public string Table { get; set; }

        public bool Equals(TableIdentifier other)
        {
            return other != null &&
                ServerId == other.ServerId &&
                Database == other.Database &&
                Schema == other.Schema &&
                Table == other.Table;
        }

        public override bool Equals(object obj)
        {
            var tbl = obj as TableIdentifier;
            if (tbl != null)
                return Equals(tbl);
            return false;
        }

        public override int GetHashCode()
        {
            return (ServerId + Database + Schema + Table).GetHashCode();
        }

        public new void Serialize(BinaryWriter stream)
        {
            base.Serialize(stream);           
            stream.Write(Table);
        }

        public new static TableIdentifier Deserialize(BinaryReader stream)
        {
            return new TableIdentifier
            {
                ServerId = stream.ReadInt16(),
                Database = stream.ReadString(),
                Schema = stream.ReadString(),
                Table = stream.ReadString()
            };
        }
    }
}
