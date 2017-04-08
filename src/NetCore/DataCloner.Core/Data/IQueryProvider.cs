using DataCloner.Core.Data.Generator;
using DataCloner.Core.Internal;
using DataCloner.Core.Metadata.Context;
using DataCloner.Core.Plan;
using System.Data;

namespace DataCloner.Core.Data
{
    public interface IQueryProvider
    {
        event QueryCommitingEventHandler QueryCommmiting;

        ISqlWriter SqlWriter { get; }

        DbEngine Engine { get; }

        object GetLastInsertedPk(IDbConnection connection);

        void EnforceIntegrityCheck(IDbConnection connection, bool active);

        object[][] Select(IDbConnection connection, Metadatas metadata, RowIdentifier row);

        void Execute(IDbConnection connection, Metadatas metadata, ExecutionPlan plan);
    }
}
