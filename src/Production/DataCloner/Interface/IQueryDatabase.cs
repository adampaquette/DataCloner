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
        bool Insert(ITableIdentifier ti, DataRow[] rows);
        bool Update(IRowIdentifier ri, DataRow[] rows);
        bool Delete(IRowIdentifier ri);
    }
}
