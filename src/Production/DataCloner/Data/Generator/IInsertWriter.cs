using System.Text;
using DataCloner.Core.Internal;
using DataCloner.Core.Metadata;
using System.Collections.Generic;

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