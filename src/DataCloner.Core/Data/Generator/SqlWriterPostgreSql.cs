using DataCloner.Core.Internal;
using System;

namespace DataCloner.Core.Data.Generator
{
    internal class SqlWriterPostgreSql : ISqlWriter
    {
        public string IdentifierDelemiterStart => "\"";
        public string IdentifierDelemiterEnd => "\"";
        public string StringDelemiter => "'";
        public string NamedParamPrefix => "@";

        public ISelectWriter GetSelectWriter() =>
            new SelectWriterPostgreSql(IdentifierDelemiterStart, IdentifierDelemiterEnd,
                                       StringDelemiter, NamedParamPrefix);

        public IInsertWriter GetInsertWriter() =>
            new InsertWriter(IdentifierDelemiterStart, IdentifierDelemiterEnd,
                             StringDelemiter, NamedParamPrefix);

        public IUpdateWriter GetUpdateWriter(UpdateStep step) =>
            new UpdateWriter(step, IdentifierDelemiterStart, IdentifierDelemiterEnd,
                             StringDelemiter, NamedParamPrefix);

        public string SelectLastIdentity(int sqlVarId, string tableName, string colName) =>
            $"SELECT {sqlVarId} K, currval(pg_get_serial_sequence('{tableName}','{colName}') V;\r\n";
    }
}

