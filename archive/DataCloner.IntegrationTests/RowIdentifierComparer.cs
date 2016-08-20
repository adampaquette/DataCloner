using System;
using System.Collections.Generic;
using System.Linq;

namespace DataCloner.Core.IntegrationTests
{
    public static class RowIdentifierComparer
    {
        public static IEqualityComparer<RowIdentifier> OrdinalIgnoreCase;

        static RowIdentifierComparer()
        {
            OrdinalIgnoreCase = new RowIdentifierComparerOrdinalIgnoreCase();
        }
    }

    public class RowIdentifierComparerOrdinalIgnoreCase : IEqualityComparer<RowIdentifier>
    {
        public bool Equals(RowIdentifier x, RowIdentifier y)
        {
            if (x.ServerId != y.ServerId ||
                !x.Database.Equals(y.Database, StringComparison.OrdinalIgnoreCase) ||
                !x.Schema.Equals(y.Schema, StringComparison.OrdinalIgnoreCase) ||
                !x.Table.Equals(y.Table, StringComparison.OrdinalIgnoreCase))
                return false;
            return Enumerable.SequenceEqual(x.Columns, y.Columns);
        }

        public int GetHashCode(RowIdentifier obj)
        {
            return obj.GetHashCode();
        }
    }   
}
