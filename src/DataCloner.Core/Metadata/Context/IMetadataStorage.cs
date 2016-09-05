using DataCloner.Core.Configuration;
using DataCloner.Core.Data;
using System.Collections.Generic;

namespace DataCloner.Core.Metadata.Context
{
    internal interface IMetadataStorage
    {
         string ConfigFileHash { get; set; }

         Dictionary<SehemaIdentifier, SehemaIdentifier> Map { get; set; }

         List<SqlConnection> ConnectionStrings { get; set; }

         Metadatas Metadatas { get; set; }

        void LoadMetadata(ConfigurationProject project, ref IQueryProxy queryProxy, CloningContext context = null);
    }
}
