using System;
using System.Data;

using DataCloner.DataClasse.Cache;

namespace DataCloner.Interface
{
    public interface IQueryProvider : IDisposable
    {
        IDbConnection Connection { get; }
        bool IsReadOnly { get; }
        string[] GetDatabasesName();
        void GetForeignKeys(Action<IDataReader, Int16, string> reader, String database);    
        DataTable GetFk(ITableIdentifier ti);
        Int64 GetLastInsertedPk();
        DataTable Select(IRowIdentifier ri);
        void Insert(ITableIdentifier ti, DataRow[] rows);
        void Update(IRowIdentifier ri, DataRow[] rows);
        void Delete(IRowIdentifier ri);
    }
}
