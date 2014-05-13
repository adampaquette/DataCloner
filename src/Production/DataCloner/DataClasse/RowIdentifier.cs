using System.Collections.Generic;
using DataCloner.Interface;

namespace DataCloner.DataClasse
{
    public class RowIdentifier : IRowIdentifier
    {
        public ITableIdentifier TableIdentifier { get; set; }
        public IDictionary<string, object> Columns { get; set; }

        public RowIdentifier()
        {
            TableIdentifier = new TableIdentifier();
            Columns = new Dictionary<string, object>();
        }
    }
}
