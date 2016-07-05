using DataCloner.Core.Internal;
using System;

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
        string AssignVarWithIdentity(string sqlVarName);
    }
}
