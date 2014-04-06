using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataCloner.DataClasse
{
    public class TableIdentifier : ITableIdentifier
    {
        public Int16 ServerID { get; set; }
        public string DatabaseName { get; set; }
        public string SchemaName { get; set; }
        public string TableName { get; set; }

        public override string ToString()
        {
            //TODO : EST UTILISÉ PAR GETHASHCODE??
            return ServerID + "." + DatabaseName + "." + SchemaName + "." + TableName;
        }
    }

    public class EqualityComparerITableIdentifier : IEqualityComparer<ITableIdentifier>
    {
        bool IEqualityComparer<ITableIdentifier>.Equals(ITableIdentifier x, ITableIdentifier y)
        {
            return x.ServerID.Equals(y.ServerID) &&
                   x.DatabaseName.Equals(y.DatabaseName) &&
                   x.SchemaName.Equals(y.SchemaName) &&
                   x.TableName.Equals(y.TableName);
        }

        int IEqualityComparer<ITableIdentifier>.GetHashCode(ITableIdentifier obj)
        {
            return (obj.ServerID.ToString() + obj.DatabaseName + obj.SchemaName + obj.TableName).GetHashCode();
        }
    }
}
