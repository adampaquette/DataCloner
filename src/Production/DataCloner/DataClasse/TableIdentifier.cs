using System;
using System.Collections.Generic;
using DataCloner.Interface;

namespace DataCloner.DataClasse
{
    public class TableIdentifier : ITableIdentifier
    {
        public Int16 ServerId { get; set; }
        public string DatabaseName { get; set; }
        public string SchemaName { get; set; }
        public string TableName { get; set; }

        public override string ToString()
        {
            //TODO : EST UTILISÉ PAR GETHASHCODE??
            return ServerId + "." + DatabaseName + "." + SchemaName + "." + TableName;
        }
    }

    public class EqualityComparerITableIdentifier : IEqualityComparer<ITableIdentifier>
    {
        bool IEqualityComparer<ITableIdentifier>.Equals(ITableIdentifier x, ITableIdentifier y)
        {
            return x.ServerId.Equals(y.ServerId) &&
                   x.DatabaseName.Equals(y.DatabaseName) &&
                   x.SchemaName.Equals(y.SchemaName) &&
                   x.TableName.Equals(y.TableName);
        }

        int IEqualityComparer<ITableIdentifier>.GetHashCode(ITableIdentifier obj)
        {
            return (obj.ServerId.ToString() + obj.DatabaseName + obj.SchemaName + obj.TableName).GetHashCode();
        }
    }
}
