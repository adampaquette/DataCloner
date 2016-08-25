using DataCloner.Core.Data;
using System.Collections.Generic;

namespace DataCloner.Core.Metadata.Context
{
    interface IMetadataStorage
    {
         string ConfigFileHash { get; set; }

         Dictionary<SehemaIdentifier, SehemaIdentifier> Map { get; set; }

         List<SqlConnection> ConnectionStrings { get; set; }

         Metadatas Metadatas { get; set; }
    }
}
