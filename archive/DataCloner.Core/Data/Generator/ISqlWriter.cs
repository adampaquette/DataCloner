using DataCloner.Core.Internal;

namespace DataCloner.Core.Data.Generator
{
    public interface ISqlWriter
    {
        string IdentifierDelemiterStart { get; }
        string IdentifierDelemiterEnd { get; }
        string StringDelemiter { get; }
        string NamedParamPrefix { get; }
        ISelectWriter GetSelectWriter();
        IInsertWriter GetInsertWriter();
        IUpdateWriter GetUpdateWriter(UpdateStep step);
        string SelectLastIdentity(int sqlVarId, string tableName, string colName);
    }
}
