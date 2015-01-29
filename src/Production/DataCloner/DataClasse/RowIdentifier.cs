using System;
using System.Collections.Generic;

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
            if ((object)obj == null)
                return false;

            if (ServerId != obj.ServerId || Database != obj.Database || Schema != obj.Schema || Table != obj.Table)
                return false;

            foreach (var col in Columns)
            {
                if (!obj.Columns.ContainsKey(col.Key) || !obj.Columns[col.Key].Equals(col.Value))
                    return false;
            }
            return true;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            var row = obj as IRowIdentifier;
            if ((object)row == null)
                return false;

            return row.Equals(this);
        }

        public override int GetHashCode()
        {
            return (Database + Schema + Table).GetHashCode();
        }
    }
}
