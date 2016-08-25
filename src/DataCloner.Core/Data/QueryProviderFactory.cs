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
                    return new MsSqlQueryProvider();//TODO : Implement singleton
                case QueryHelperMySql.ProviderName:
                    return new QueryHelperMySql();//TODO : Implement singleton
                case PostgreSqlQueryProvider.ProviderName:
                    return new PostgreSqlQueryProvider();//TODO : Implement singleton
            }
            throw new NotSupportedException($"Unkown provider : {providerName}");
        }
    }
}
