using DataCloner.Metadata;

namespace DataCloner.Internal
{
    internal class CircularKeyJob
    {
        public RowIdentifier SourceBaseRowStartPoint { get; set; }
        public RowIdentifier SourceFkRowStartPoint { get; set; }
        public ForeignKey ForeignKey { get; set; }
    }
}
