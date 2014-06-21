using System;
using System.Data;

namespace DataCloner.Interface
{
    public interface IQueryProvider : IDisposable
    {
        IDbConnection Connection { get; }
        bool IsReadOnly { get; }
        DataTable GetFk(ITableIdentifier ti);
        Int64 GetLastInsertedPk();
        DataTable Select(IRowIdentifier ri);
        void Insert(ITableIdentifier ti, DataRow[] rows);
        void Update(IRowIdentifier ri, DataRow[] rows);
        void Delete(IRowIdentifier ri);
    }
}
