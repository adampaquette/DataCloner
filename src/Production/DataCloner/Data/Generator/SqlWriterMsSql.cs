using DataCloner.Core.Internal;

namespace DataCloner.Core.Data.Generator
{
    internal class SqlWriterMsSql : ISqlWriter
    {
        public string IdentifierDelemiterStart => "[";
        public string IdentifierDelemiterEnd => "]";
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

        public string AssignVarWithIdentity(string sqlVarName) =>
            "SET " + NamedParamPrefix + sqlVarName + " = SCOPE_IDENTITY();\r\n";
    }
}