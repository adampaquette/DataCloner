using DataCloner.Core.Internal;

namespace DataCloner.Core.Data.Generator.MySql
{
    internal class MySqlWriter : ISqlWriter
    {
        public string IdentifierDelemiterStart => "`";
        public string IdentifierDelemiterEnd => "`";
        public string StringDelemiter => "'";
        public string NamedParamPrefix => "@";

        public ISelectWriter GetSelectWriter() =>
            new SelectWriter(IdentifierDelemiterStart, IdentifierDelemiterEnd);

        public IInsertWriter GetInsertWriter() =>
            new InsertWriter(IdentifierDelemiterStart, IdentifierDelemiterEnd,
                             StringDelemiter, NamedParamPrefix);

        public IUpdateWriter GetUpdateWriter(UpdateStep step) =>
            new UpdateWriter(step, IdentifierDelemiterStart, IdentifierDelemiterEnd);

        /// <summary>
        /// Declare and select the last primary key generated.
        /// The connection string MUST have this option "Allow User Variables=True".
        /// </summary>
        /// <param name="sqlVarId">Var id generated internally.</param>
        /// <param name="tableName">Table name</param>
        /// <param name="colName">Column name</param>
        /// <returns>Sql query</returns>
        public string SelectLastIdentity(int sqlVarId, string tableName, string colName) =>    
            $"SET {NamedParamPrefix}{sqlVarId} = LAST_INSERT_ID();\r\n" +
            $"SELECT {sqlVarId} K, {NamedParamPrefix}{sqlVarId} V;\r\n";
    }
}