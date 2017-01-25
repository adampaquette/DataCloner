using DataCloner.Core.Data.Generator;
using DataCloner.Core.Metadata.Context;
using System.Collections.Generic;
using System.Data;

namespace DataCloner.Core.Data
{
    internal abstract class MetadataProvider : IMetadataProvider
    {
        /// <summary>
        /// SQL statement to query database's name.
        /// </summary>
        protected abstract string SqlGetDatabasesName { get; }

        /// <summary>
        /// SQL statement to query columns.
        /// </summary>
        protected abstract string SqlGetColumns { get; }

        /// <summary>
        /// SQL statement to query foreign keys.
        /// </summary>
        protected abstract string SqlGetForeignKeys { get; }

        /// <summary>
        /// SQL statement to query unique keys.
        /// </summary>
        protected abstract string SqlGetUniqueKeys { get; }

        public abstract DbEngine Engine { get; }

        public abstract ISqlTypeConverter TypeConverter { get; }

        public abstract ISqlWriter SqlWriter { get; }

        public string[] GetDatabasesName(IDbConnection connection)
        {
            var databases = new List<string>();

            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = SqlGetDatabasesName;
                connection.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                        databases.Add(r.GetString(0));
                }
                connection.Close();
            }
            return databases.ToArray();
        }

        public void LoadColumns(IDbConnection connection, Metadatas metadata, string serverId, string database)
        {
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = SqlGetColumns;

                var p = cmd.CreateParameter();
                p.ParameterName = SqlWriter.NamedParamPrefix + "DATABASE";
                p.Value = database;
                cmd.Parameters.Add(p);

                connection.Open();
                using (var r = cmd.ExecuteReader())
                    MetadataLoader.LoadColumns(r, metadata, serverId, database, TypeConverter);
                connection.Close();
            }
        }

        public void LoadForeignKeys(IDbConnection connection, Metadatas metadata, string serverId, string database)
        {
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = SqlGetForeignKeys;

                var p = cmd.CreateParameter();
                p.ParameterName = SqlWriter.NamedParamPrefix + "DATABASE";
                p.Value = database;
                cmd.Parameters.Add(p);

                connection.Open();
                using (var r = cmd.ExecuteReader())
                    MetadataLoader.LoadForeignKeys(r, metadata, serverId, database);
                connection.Close();
            }
        }

        public void LoadUniqueKeys(IDbConnection connection, Metadatas metadata, string serverId, string database)
        {
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = SqlGetUniqueKeys;

                var p = cmd.CreateParameter();
                p.ParameterName = SqlWriter.NamedParamPrefix + "DATABASE";
                p.Value = database;
                cmd.Parameters.Add(p);

                connection.Open();
                using (var r = cmd.ExecuteReader())
                    MetadataLoader.LoadUniqueKeys(r, metadata, serverId, database);
                connection.Close();
            }
        }
    }
}
