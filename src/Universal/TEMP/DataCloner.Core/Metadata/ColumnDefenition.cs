using DataCloner.Core.Data;
using System.Data;

namespace DataCloner.Core.Metadata
{
    public sealed class ColumnDefinition
    {
        public string Name { get; set; }

        public DbType DbType { get; set; }

        public SqlType SqlType { get; set; }

        public bool IsPrimary { get; set; }

        public bool IsForeignKey { get; set; }

        public bool IsUniqueKey { get; set; }

        public bool IsAutoIncrement { get; set; }

        public string BuilderName { get; set; }

        public ColumnDefinition()
        {
            SqlType = new SqlType();
        }
    }
}
