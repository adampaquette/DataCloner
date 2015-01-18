using System.Collections.Generic;

namespace DataCloner.DataClasse
{
    public class ColumnIdentifier : TableIdentifier, IColumnIdentifier
    {
        public string Column { get; set; }
    }

    public class EqualityComparerIColumnIdentifier : IEqualityComparer<IColumnIdentifier>
    {
        bool IEqualityComparer<IColumnIdentifier>.Equals(IColumnIdentifier x, IColumnIdentifier y)
        {
            return x.ServerId.Equals(y.ServerId) &&
                   x.Database.Equals(y.Database) &&
                   x.Schema.Equals(y.Schema) &&
                   x.Table.Equals(y.Table) &&
                   x.Column.Equals(y.Column);
        }

        int IEqualityComparer<IColumnIdentifier>.GetHashCode(IColumnIdentifier obj)
        {
            return (obj.ServerId + obj.Database + obj.Schema + obj.Table + obj.Column).GetHashCode();
        }
    }
}
