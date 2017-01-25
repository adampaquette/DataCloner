namespace DataCloner.Core.Metadata
{
    public sealed class DerivativeTable
    {
        public string ServerId { get; set; }

        public string Database { get; set; }

        public string Schema { get; set; }

        public string Table { get; set; }

        public DerivativeTableAccess Access { get; set; }

        public bool Cascade { get; set; }

        public override bool Equals(object obj)
        {
            var tableToObj = obj as DerivativeTable;
            if (tableToObj == null)
                return false;
            return ServerId.Equals(tableToObj.ServerId) &&
                   Database.Equals(tableToObj.Database) &&
                   Schema.Equals(tableToObj.Schema) &&
                   Table.Equals(tableToObj.Table);
        }

        public override int GetHashCode()
        {
            return Table.GetHashCode();
        }
    }
}
