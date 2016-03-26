using DataCloner.Core.Configuration;

namespace DataCloner.Core
{
    public class Settings
    {
        public ProjectContainer Project { get; set; }
        public int? MapId { get; set; }
        public int? BehaviourId { get; set; }
        public bool UseInMemoryCacheOnly { get; set; }
    }
}
