using DataCloner.Core.Configuration;

namespace DataCloner.Core.Metadata.Context
{
    internal interface IMetadataCache
    {
         string ConfigFileHash { get; set; }

         Metadatas Metadatas { get; set; }

        ExecutionContext LoadCache(ConfigurationProject project, CloningContext context = null);
    }
}
