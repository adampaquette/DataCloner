using System;
using System.Data;
using DataCloner.DataClasse;

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
        string[] GetDatabasesName();
        void GetColumns(ColumnReader reader, String database);
        void GetForeignKeys(ForeignKeyReader reader, String database);
        void GetUniqueKeys(UniqueKeyReader reader, string database);
        object GetLastInsertedPk();
        void EnforceIntegrityCheck(bool active);
        object[][] Select(IRowIdentifier ri);
        void Insert(ITableIdentifier ti, object[] row);
        void Update(IRowIdentifier riDictionary, ColumnsWithValue values);
        void Delete(IRowIdentifier ri);
        void SqlTypeToDbType(string fullType, out DbType type, out string size);
    }
}
