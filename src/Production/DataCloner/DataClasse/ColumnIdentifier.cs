using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Interface;

namespace DataClasse
{
    public class ColumnIdentifier : TableIdentifier, IColumnIdentifier
    {
        public string ColumnName { get; set; }
    }

    public class EqualityComparerIColumnIdentifier : IEqualityComparer<IColumnIdentifier>
    {
        bool IEqualityComparer<IColumnIdentifier>.Equals(IColumnIdentifier x, IColumnIdentifier y)
        {
            return x.ServerID.Equals(y.ServerID) &&
                   x.DatabaseName.Equals(y.DatabaseName) &&
                   x.SchemaName.Equals(y.SchemaName) &&
                   x.TableName.Equals(y.TableName) &&
                   x.ColumnName.Equals(y.ColumnName);
        }

        int IEqualityComparer<IColumnIdentifier>.GetHashCode(IColumnIdentifier obj)
        {
            return (obj.ServerID + obj.DatabaseName + obj.SchemaName + obj.TableName + obj.ColumnName).GetHashCode();
        }
    }
}
