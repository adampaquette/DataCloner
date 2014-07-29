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
        void GetColumns(Action<IDataReader, Int16, string> reader, String database);
        void GetForeignKeys(Action<IDataReader, Int16, string> reader, String database);
        //Int64 GetLastInsertedPk();
        object[][] Select(IRowIdentifier ri);
        void Insert(ITableIdentifier ti, object[] row);
        void Update(IRowIdentifier ri, DataRow[] rows);
        void Delete(IRowIdentifier ri);
    }
}
