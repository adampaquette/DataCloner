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
            switch (providerName)
            {
                case MsSqlQueryProvider.ProviderName:
                    return MsSqlMetadataProvider.Instance; 
                case MySqlQueryProvider.ProviderName:
                    return MySqlMetadataProvider.Instance;
                case PostgreSqlQueryProvider.ProviderName:
                    return PostgreSqlMetadataProvider.Instance;
                default:
                    throw new NotSupportedException($"Unkown provider : {providerName}");
            }
        }
    }
}
