using System.Text;
using DataCloner.DataClasse;
using DataCloner.DataClasse.Cache;

namespace DataCloner.Generator
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