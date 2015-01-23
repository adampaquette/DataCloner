using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataCloner.DataClasse.Cache;
using System.Data.Common;

namespace DataCloner.DataAccess
{
    internal static class QueryHelperFactory 
    {
        public static IQueryHelper GetQueryHelper(string providerName, string connectionString, Int16 serverId, Configuration cache)
        {
            switch (providerName)
            {
                //case SqlServerProvider.ProviderName:
                //    return new SqlServerProvider();
                case QueryHelperMySql.ProviderName:
                    return new QueryHelperMySql(connectionString, serverId, cache);
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

        public static IQueryHelper GetQueryHelper(this DbConnection cnx, Int16 serverId, Configuration cache)
        {
            var type = cnx.GetType().Name;

            //if (type.Equals("SqlConnection", StringComparison.InvariantCultureIgnoreCase))
            //    return new SqlServerProvider();

            if (type.StartsWith("MySql"))
                return new QueryHelperMySql(cnx.ConnectionString, serverId, cache);

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
