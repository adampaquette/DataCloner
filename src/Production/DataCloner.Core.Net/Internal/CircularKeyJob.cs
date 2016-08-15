using DataCloner.Core.Metadata;

namespace DataCloner.Core.Internal
{
    internal class CircularKeyJob
    {
        public RowIdentifier SourceBaseRowStartPoint { get; set; }
        public RowIdentifier SourceFkRowStartPoint { get; set; }
        public ForeignKey ForeignKey { get; set; }
    }
}
