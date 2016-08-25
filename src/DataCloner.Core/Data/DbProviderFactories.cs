using DataCloner.Core.Data.MsSql;
using System;
using System.Data.Common;
using System.Data.SqlClient;

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
            DbProviderFactory factory;
            switch (providerName)
            {
                case MsSqlQueryProvider.ProviderName:
                    factory = SqlClientFactory.Instance;
                    break;
                default:
                    throw new NotSupportedException($"Provider not supported : {providerName}");
            }
            return factory;
        }
    }
}
