using System;
using System.Diagnostics;
using System.IO;

namespace DataCloner.Core
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

        public new void Serialize(BinaryWriter output)
        {
            base.Serialize(output);           
            output.Write(Table);
        }

        public new static TableIdentifier Deserialize(BinaryReader input)
        {
            return new TableIdentifier
            {
                ServerId = input.ReadInt16(),
                Database = input.ReadString(),
                Schema = input.ReadString(),
                Table = input.ReadString()
            };
        }
    }
}
