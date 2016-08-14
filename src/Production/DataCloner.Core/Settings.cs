using DataCloner.Core.Configuration;

namespace DataCloner.Core
{
    public class Settings
    {
        public ProjectContainer Project { get; set; }
        public int? MapId { get; set; }
        public int? BehaviourId { get; set; }
        /// <summary>
        /// We don't load metadata cache from disk nor save it.
        /// </summary>
        public bool UseInMemoryCacheOnly { get; set; }
    }
}
