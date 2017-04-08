using System.Collections.Generic;

namespace DataCloner.Core.Metadata
{
    public sealed class ForeignKey
    {
        public string ServerIdTo { get; set; }

        public string DatabaseTo { get; set; }

        public string SchemaTo { get; set; }

        public string TableTo { get; set; }

        public List<ForeignKeyColumn> Columns { get; set; }
    }
}
