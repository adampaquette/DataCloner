using System;
using System.Data;
using DataCloner.DataClasse;
using DataCloner.DataClasse.Cache;

namespace DataCloner.DataAccess
{
    public interface IQueryDispatcher
    {
        IQueryHelper this[IServerIdentifier server] { get; }
        IQueryHelper this[Int16 server] { get; }
        IDbConnection GetConnection(IServerIdentifier server);
        IDbConnection GetConnection(Int16 server);
        IQueryHelper GetQueryHelper(IServerIdentifier server);
        IQueryHelper GetQueryHelper(Int16 server);

        void InitProviders(Cache cache);
    }
}
