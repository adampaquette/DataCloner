using System;
using System.Linq;

namespace DataCloner.DataClasse
{
    public class RowIdentifier : IRowIdentifier
    {
        public Int16 ServerId { get; set; }
        public string Database { get; set; }
        public string Schema { get; set; }
        public string Table { get; set; }
        public ColumnsWithValue Columns { get; set; }

        public RowIdentifier()
        {
            Columns = new ColumnsWithValue();
        }

        public bool Equals(IRowIdentifier obj)
        {
            if (ServerId != obj?.ServerId || Database != obj.Database || Schema != obj.Schema || Table != obj.Table)
                return false;

            return Columns.All(col => obj.Columns.ContainsKey(col.Key) && obj.Columns[col.Key].Equals(col.Value));
        }

        public override bool Equals(object obj)
        {
            var row = obj as IRowIdentifier;
            if (row == null)
                return false;

            return row.Equals(this);
        }

        public override int GetHashCode()
        {
            return (Database + Schema + Table).GetHashCode();
        }
    }
}
