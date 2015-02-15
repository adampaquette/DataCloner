using System;
using System.Data.Common;
using DataCloner.DataClasse.Cache;

namespace DataCloner.DataAccess
{
    internal static class QueryHelperFactory 
    {
        public static IQueryHelper GetQueryHelper(Cache cache, string providerName, string connectionString, Int16 serverId)
        {
            switch (providerName)
            {
                //case SqlServerProvider.ProviderName:
                //    return new SqlServerProvider();
                case QueryHelperMySql.ProviderName:
                    return new QueryHelperMySql(cache, connectionString, serverId);
                //case PostgresProvider.ProviderName:
                //    return new PostgresProvider();
                //case OracleProvider.ProviderName:
                //    return new OracleProvider();
                //case SqlServerCEProvider.ProviderName:
                //    return new SqlServerCEProvider();
                //case SqliteProvider.ProviderName:
                //    return new SqliteProvider();
            }
            throw new Exception("Unkown provider");
        }

        public static IQueryHelper GetQueryHelper(this DbConnection cnx, Cache cache, Int16 serverId)
        {
            var type = cnx.GetType().Name;

            //if (type.Equals("SqlConnection", StringComparison.InvariantCultureIgnoreCase))
            //    return new SqlServerProvider();

            if (type.StartsWith("MySql"))
                return new QueryHelperMySql(cache, cnx.ConnectionString, serverId);

            //if (type.StartsWith("Npgsql"))
            //    return new PostgresProvider();

            //if (type.StartsWith("SQLite"))
            //    return new SqliteProvider();

            //if (type.Equals("SqlCeConnection", StringComparison.InvariantCultureIgnoreCase))
            //    return new SqlServerCEProvider();

            throw new NotSupportedException();
        }
    }
}
