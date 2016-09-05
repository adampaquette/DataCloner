using DataCloner.Core.Data.Generator;
using DataCloner.Core.Metadata.Context;
using System.Data;

namespace DataCloner.Core.Data
{
    public interface IMetadataProvider
    {
        string[] GetDatabasesName(IDbConnection connection);

        void LoadColumns(IDbConnection connection, Metadatas metadata, short serverId, string database);

        void LoadForeignKeys(IDbConnection connection, Metadatas metadata, short serverId, string database);

        void LoadUniqueKeys(IDbConnection connection, Metadatas metadata, short serverId, string database);

        ISqlTypeConverter TypeConverter { get; }

        ISqlWriter SqlWriter { get; }

        DbEngine Engine { get; }        
    }
}
