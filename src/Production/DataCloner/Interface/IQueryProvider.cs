using System;
using System.Data;

using DataCloner.DataClasse.Cache;

namespace DataCloner.Interface
{

    public delegate void ColumnReader(IDataReader reader, Int16 serverId, string database, Func<string, Type> sqlToClrDatatype);

    public interface IQueryProvider : IDisposable
    {
        IDbConnection Connection { get; }
        bool IsReadOnly { get; }
        string[] GetDatabasesName();
        void GetColumns(ColumnReader reader, String database);
        void GetForeignKeys(Action<IDataReader, Int16, string> reader, String database);
        object GetLastInsertedPk();
        object[][] Select(IRowIdentifier ri);
        void Insert(ITableIdentifier ti, object[] row);
        void Update(IRowIdentifier ri, DataRow[] rows);
        void Delete(IRowIdentifier ri);
        Type SqlToClrDatatype(string type);
    }
}
