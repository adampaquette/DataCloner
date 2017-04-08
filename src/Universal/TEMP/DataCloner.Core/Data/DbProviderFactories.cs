using System;
using System.Data.Common;
using System.Data.SqlClient;
using DataCloner.Core.Data.MsSql;
using DataCloner.Core.Data.MySql;
using DataCloner.Core.Data.PostgreSql;
//using MySql.Data.MySqlClient;
//using Npgsql;

namespace DataCloner.Core.Data
{
    /// <summary>
    /// Methods giving the ability to create an instance of a factory for a data source. 
    /// </summary>
    public static class DbProviderFactories
    {
        /// <summary>
        /// Create an instance of a factory for a data source. 
        /// </summary>
        /// <param name="providerName">provider</param>
        /// <returns>DbProviderFactory</returns>
        public static DbProviderFactory GetFactory(string providerName)
        {
            switch (providerName)
            {
                case MsSqlQueryProvider.ProviderName:
                    return SqlClientFactory.Instance;
                case PostgreSqlQueryProvider.ProviderName:
                    //return NpgsqlFactory.Instance;
                case MySqlQueryProvider.ProviderName:
                    //return MySqlClientFactory.Instance;
                default:
                    throw new NotSupportedException($"Provider not supported : {providerName}");
            }
        }
    }
}
