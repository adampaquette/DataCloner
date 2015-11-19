using DataCloner.Metadata;

namespace DataCloner.Internal
{
    internal class CircularKeyJob
    {
        public IRowIdentifier SourceBaseRowStartPoint { get; set; }
        public IRowIdentifier SourceFkRowStartPoint { get; set; }
        public ForeignKey ForeignKey { get; set; }
    }
}
