using System;

namespace DataCloner.Generator
{
    internal class SqlWriterMySql : AbstractSqlWriter
    {
        public override IInsertWriter GetInsertWriter()
        {
            throw new NotImplementedException();
        }
    }
}
