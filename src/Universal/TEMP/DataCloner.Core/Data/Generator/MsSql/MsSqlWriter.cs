﻿using DataCloner.Core.Internal;

namespace DataCloner.Core.Data.Generator.MsSql
{
    internal class MsSqlWriter : ISqlWriter
    {
        public string IdentifierDelemiterStart => "[";
        public string IdentifierDelemiterEnd => "]";
        public string StringDelemiter => "'";
        public string NamedParamPrefix => "@";

        public ISelectWriter GetSelectWriter() =>
            new SelectWriter(IdentifierDelemiterStart, IdentifierDelemiterEnd);

        public IInsertWriter GetInsertWriter() =>
            new InsertWriter(IdentifierDelemiterStart, IdentifierDelemiterEnd,
                             StringDelemiter, NamedParamPrefix);

        public IUpdateWriter GetUpdateWriter(UpdateStep step) =>
            new UpdateWriter(step, IdentifierDelemiterStart, IdentifierDelemiterEnd);

        public string SelectLastIdentity(int sqlVarId, string tableName, string colName) =>
            $"DECLARE {NamedParamPrefix}{sqlVarId} VARCHAR(MAX);" +        
            $"SET {NamedParamPrefix}{sqlVarId} = SCOPE_IDENTITY();\r\n" +
            $"SELECT {sqlVarId} K, {NamedParamPrefix}{sqlVarId} V;\r\n";
    }
}