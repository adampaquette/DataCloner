using DataCloner.Core.Data.MsSql;
using DataCloner.Core.Data.MySql;
using DataCloner.Core.Data.PostgreSql;
using System;

namespace DataCloner.Core.Data
{
    internal static class QueryProviderFactory 
    {
        public static IQueryProvider GetProvider(string providerName)
        {
            switch (providerName)
            {
                case MsSqlQueryProvider.ProviderName:
                    return MsSqlQueryProvider.Instance;
                case QueryHelperMySql.ProviderName:
                    return QueryHelperMySql.Instance;
                case PostgreSqlQueryProvider.ProviderName:
                    return PostgreSqlQueryProvider.Instance;
            }
            throw new NotSupportedException($"Unkown provider : {providerName}");
        }
    }
}
