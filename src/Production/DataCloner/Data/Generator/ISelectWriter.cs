using DataCloner.Core.Metadata;
using System.Collections.Generic;
using System.Text;

namespace DataCloner.Core.Data.Generator
{
    public interface ISelectWriter
    {
        ISelectWriter AppendColumns(RowIdentifier row, List<ColumnDefinition> columns);
        ISelectWriter AppendToWhere(string colName, string paramName);
        StringBuilder ToStringBuilder();
    }
}