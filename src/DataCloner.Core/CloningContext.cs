namespace DataCloner.Core
{
    public class CloningContext
    {
        public string From { get; set; }

        public string To { get; set; }

        public short? BehaviourId { get; set; }

        /// <summary>
        /// When set to true the metadata's cache is not persisted over time. It is not loaded from or to the disk.
        /// </summary>
        public bool UseInMemoryCacheOnly { get; set; }
    }
}
