using DataCloner.Core.Metadata;
using System.Collections.Generic;
using System.Text;

namespace DataCloner.Core.Data.Generator
{
    public interface IInsertWriter
    {
        IInsertWriter Append(string value);
        IInsertWriter AppendColumns(TableIdentifier table, List<ColumnDefinition> columns);
        IInsertWriter AppendValue(object value);
        IInsertWriter AppendVariable(string varName);
        IInsertWriter Complete();
        StringBuilder ToStringBuilder();
    }
}