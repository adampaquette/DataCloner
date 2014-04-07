using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace DataCloner
{
    public interface IQueryDatabase  : IDisposable
    {
        IDbConnection Connection { get; }
        bool IsReadOnly { get; }
        DataTable GetFK(ITableIdentifier ti);
        Int64 GetLastInsertedPK();
        DataTable Select(IRowIdentifier ri);
        void Insert(ITableIdentifier ti, DataRow[] rows);
        void Update(IRowIdentifier ri, DataRow[] rows);
        void Delete(IRowIdentifier ri);
    }
}
