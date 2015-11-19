using System.Text;
using DataCloner.Internal;
using DataCloner.Metadata;
using System.Collections.Generic;

namespace DataCloner.Data.Generator
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