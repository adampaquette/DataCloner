using DataCloner.Core.Data;
using System.Data;

namespace DataCloner.Core
{
    public class ConnectionContext
    {
        public IDbConnection Connection { get; }

        public IQueryProvider QueryProvider { get; }

        public ConnectionContext(IDbConnection connection, IQueryProvider queryProvider)
        {
            Connection = connection;
            QueryProvider = queryProvider;
        }
    }
}
