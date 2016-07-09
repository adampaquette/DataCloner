using DataCloner.Core.Internal;
using System;

namespace DataCloner.Core.Data.Generator
{
    internal class SqlWriterMySql : ISqlWriter
    {
        public string IdentifierDelemiterStart => "`";
        public string IdentifierDelemiterEnd => "`";
        public string StringDelemiter => "'";
        public string NamedParamPrefix => "@";

        public ISelectWriter GetSelectWriter() =>
            new SelectWriter(IdentifierDelemiterStart, IdentifierDelemiterEnd,
                             StringDelemiter, NamedParamPrefix);

        public IInsertWriter GetInsertWriter() =>
            new InsertWriter(IdentifierDelemiterStart, IdentifierDelemiterEnd,
                             StringDelemiter, NamedParamPrefix);

        public IUpdateWriter GetUpdateWriter(UpdateStep step) =>
            new UpdateWriter(step, IdentifierDelemiterStart, IdentifierDelemiterEnd,
                             StringDelemiter, NamedParamPrefix);

        public string AssignVarWithIdentity(string sqlVarName)
        {
            throw new NotImplementedException();
        }           
    }
}
