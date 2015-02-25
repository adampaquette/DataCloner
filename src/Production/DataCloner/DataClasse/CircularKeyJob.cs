using DataCloner.DataClasse.Cache;

namespace DataCloner.DataClasse
{
    internal class CircularKeyJob
    {
        public IRowIdentifier SourceBaseRowStartPoint { get; set; }
        public IRowIdentifier SourceFkRowStartPoint { get; set; }
        public IForeignKey ForeignKey { get; set; }
    }
}
