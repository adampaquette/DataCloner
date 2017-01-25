using DataCloner.Core.Configuration;
using System.Collections.Generic;

namespace DataCloner.Core.Plan
{
    internal interface IExecutionContext
    {
        string ExecutionContextCacheHash { get; set; }

        Dictionary<SehemaIdentifier, SehemaIdentifier> Map { get; set; }

        ConnectionsContext ConnectionsContext { get; set; }

        void Initialize(Project project, CloningContext context = null);
    }
}
