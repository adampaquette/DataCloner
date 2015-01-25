using System;
using System.Data;

using DataCloner.DataClasse.Cache;

namespace DataCloner.DataAccess
{
    public delegate void ColumnReader(IDataReader reader, Int16 serverId, string database, SqlTypeToDbTypeConverter sqlToClrDatatype);
    public delegate void ForeignKeyReader(IDataReader reader, Int16 serverId, string database);
    public delegate void UniqueKeyReader(IDataReader reader, Int16 serverId, string database);
    public delegate void SqlTypeToDbTypeConverter(string fullType, out DbType type, out string size);
    
    public interface IQueryHelper : IDisposable
    {
        IDbConnection Connection { get; }
        DbEngine Engine { get; }
        bool IsReadOnly { get; }
        string[] GetDatabasesName();
        void GetColumns(ColumnReader reader, String database);
        void GetForeignKeys(ForeignKeyReader reader, String database);
        void GetUniqueKeys(UniqueKeyReader reader, string database);
        object GetLastInsertedPk();
        object[][] Select(IRowIdentifier ri);
        void Insert(ITableIdentifier ti, object[] row);
        void Update(IRowIdentifier ri, DataRow[] rows);
        void Delete(IRowIdentifier ri);
        void SqlTypeToDbType(string fullType, out DbType type, out string size);
    }
}
