using System.Text;
using DataCloner.Internal;
using DataCloner.Metadata;

namespace DataCloner.Data.Generator
{
    public interface IInsertWriter
    {
        IInsertWriter Append(string value);
        IInsertWriter AppendColumns(ITableIdentifier table, IColumnDefinition[] columns);
        IInsertWriter AppendValue(object value);
        IInsertWriter AppendVariable(string varName);
        IInsertWriter Complete();
        StringBuilder ToStringBuilder();
    }
}