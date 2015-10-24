using System;

namespace DataCloner.Data.Generator
{
    internal class SqlWriterMySql : AbstractSqlWriter
    {
        public override IInsertWriter GetInsertWriter()
        {
            throw new NotImplementedException();
        }
    }
}
