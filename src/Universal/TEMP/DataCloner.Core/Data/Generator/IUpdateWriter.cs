using System.Text;

namespace DataCloner.Core.Data.Generator
{
    public interface IUpdateWriter
    {
        IUpdateWriter AppendToSet(string colName, string paramName);
        IUpdateWriter AppendToWhere(string colName, string paramName);
        IUpdateWriter Complete();
        StringBuilder ToStringBuilder();
    }
}
