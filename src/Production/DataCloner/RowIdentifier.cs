using System;
using System.Diagnostics;
using System.Linq;

namespace DataCloner
{
    [DebuggerDisplay("{ServerId.ToString() + \".\" + Database + \".\" + Schema + \".\" + Table}...")]
    public class RowIdentifier : TableIdentifier, IEquatable<RowIdentifier>
    {
        public ColumnsWithValue Columns { get; set; }

        public RowIdentifier()
        {
            Columns = new ColumnsWithValue();
        }

        public RowIdentifier Clone()
        {
            var clone = new RowIdentifier
            {
                ServerId = ServerId,
                Database = Database,
                Schema = Schema,
                Table = Table
            };
            
            //TODO : DEEP CLONE VALUE
            foreach (var col in Columns)
                clone.Columns.Add(col.Key, col.Value);

            return clone;
        }

        public bool Equals(RowIdentifier obj)
        {
            if (obj == null || 
                ServerId != obj.ServerId || 
                Database != obj.Database || 
                Schema != obj.Schema || 
                Table != obj.Table)
                return false;

            return Columns.All(col => obj.Columns.ContainsKey(col.Key) && obj.Columns[col.Key].Equals(col.Value));
        }

        public override bool Equals(object obj)
        {
            var row = obj as RowIdentifier;
            if (row == null)
                return false;

            return row.Equals(this);
        }

        public override int GetHashCode()
        {
            return (ServerId + Database + Schema + Table).GetHashCode();
        }
    }
}
