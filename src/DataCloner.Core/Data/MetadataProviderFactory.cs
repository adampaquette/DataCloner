using DataCloner.Core.Data.MsSql;
using DataCloner.Core.Data.MySql;
using DataCloner.Core.Data.PostgreSql;
using System;

namespace DataCloner.Core.Data
{
    internal class MetadataProviderFactory
    {
        public static MetadataProvider GetProvider(string providerName)
        {
            MetadataProvider provider;
            switch (providerName)
            {
                case MsSqlQueryProvider.ProviderName:
                    provider = new MsSqlMetadataProvider(); //TODO : Implement singleton
                    break;
                case QueryHelperMySql.ProviderName:
                    provider = new MySqlMetadataProvider(); //TODO : Implement singleton
                    break;
                case PostgreSqlQueryProvider.ProviderName:
                    provider = new PostgreSqlMetadataProvider(); //TODO : Implement singleton
                    break;
                default:
                    throw new NotSupportedException($"Unkown provider : {providerName}");
            }
            return provider;
        }
    }
}
