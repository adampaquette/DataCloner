using System;
using System.Data;
using DataCloner.DataClasse;
using DataCloner.Generator;

namespace DataCloner.DataAccess
{
    public delegate void ColumnReader(IDataReader reader, Int16 serverId, string database, ISqlTypeConverter typeConverter);
    public delegate void ForeignKeyReader(IDataReader reader, Int16 serverId, string database);
    public delegate void UniqueKeyReader(IDataReader reader, Int16 serverId, string database);
    public delegate void SqlToClrTypeConverter(string fullType, out DbType type, out string size);

    public interface IQueryHelper : IDisposable
    {
        event QueryCommitingEventHandler QueryCommmiting;
        IDbConnection Connection { get; }
        ISqlTypeConverter TypeConverter { get; }
        ISqlWriter SqlWriter { get; }
        DbEngine Engine { get; }
        string[] GetDatabasesName();
        void GetColumns(ColumnReader reader, String database);
        void GetForeignKeys(ForeignKeyReader reader, String database);
        void GetUniqueKeys(UniqueKeyReader reader, string database);
        object GetLastInsertedPk();
        void EnforceIntegrityCheck(bool active);
        object[][] Select(IRowIdentifier row);
        void Execute(ExecutionPlan plan);
    }
}
