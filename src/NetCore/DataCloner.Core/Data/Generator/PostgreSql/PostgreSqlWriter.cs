using DataCloner.Core.Internal;

namespace DataCloner.Core.Data.Generator.PostgreSql
{
    internal class PostgreSqlWriter : ISqlWriter
    {
        public string IdentifierDelemiterStart => "\"";
        public string IdentifierDelemiterEnd => "\"";
        public string StringDelemiter => "'";
        public string NamedParamPrefix => "@";

        public ISelectWriter GetSelectWriter() =>
            new PostgreSqlSelectWriter(IdentifierDelemiterStart, IdentifierDelemiterEnd);

        public IInsertWriter GetInsertWriter() =>
            new InsertWriter(IdentifierDelemiterStart, IdentifierDelemiterEnd,
                             StringDelemiter, NamedParamPrefix);

        public IUpdateWriter GetUpdateWriter(UpdateStep step) =>
            new UpdateWriter(step, IdentifierDelemiterStart, IdentifierDelemiterEnd);

        public string SelectLastIdentity(int sqlVarId, string tableName, string colName) =>
            $"SELECT {sqlVarId} K, currval(pg_get_serial_sequence('{tableName}','{colName}') V;\r\n";
    }
}

