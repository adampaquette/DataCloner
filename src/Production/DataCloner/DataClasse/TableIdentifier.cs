using System;
using System.Collections.Generic;
using DataCloner.Interface;

namespace DataCloner.DataClasse
{
    public class TableIdentifier : ITableIdentifier
    {
        public Int16 ServerId { get; set; }
        public string Database { get; set; }
        public string Schema { get; set; }
        public string Table { get; set; }

        public override string ToString()
        {
            //TODO : EST UTILISÉ PAR GETHASHCODE??
            return ServerId + "." + Database + "." + Schema + "." + Table;
        }
    }

    public class EqualityComparerITableIdentifier : IEqualityComparer<ITableIdentifier>
    {
        bool IEqualityComparer<ITableIdentifier>.Equals(ITableIdentifier x, ITableIdentifier y)
        {
            return x.ServerId.Equals(y.ServerId) &&
                   x.Database.Equals(y.Database) &&
                   x.Schema.Equals(y.Schema) &&
                   x.Table.Equals(y.Table);
        }

        int IEqualityComparer<ITableIdentifier>.GetHashCode(ITableIdentifier obj)
        {
            return (obj.ServerId.ToString() + obj.Database + obj.Schema + obj.Table).GetHashCode();
        }
    }
}
