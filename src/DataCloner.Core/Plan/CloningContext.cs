namespace DataCloner.Core.Plan
{
    public class CloningContext
    {
        public string SourceEnvironment { get; set; }

        public string DestinationEnvironment { get; set; }

        public string Behaviour { get; set; }

        /// <summary>
        /// When set to true the metadata's cache is not persisted over time. It is not loaded from or to the disk.
        /// </summary>
        public bool UseInMemoryCacheOnly { get; set; }
    }
}
