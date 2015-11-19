using System;
using System.Collections.Generic;
using System.Diagnostics;

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
    }
}
