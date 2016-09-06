using DataCloner.Core.Metadata.Context;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace DataCloner.Core
{
    public class ExecutionContext
    {
        public Dictionary<SehemaIdentifier, SehemaIdentifier> Map { get; set; }
        public Dictionary<short, IDbConnection> DbConnections { get; set; }
        public Metadatas Metadatas { get; set; }
    }
}
