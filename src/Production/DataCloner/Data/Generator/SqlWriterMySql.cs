using System;

namespace DataCloner.Core.Data.Generator
{
    internal class SqlWriterMySql : AbstractSqlWriter
    {
        public override IInsertWriter GetInsertWriter()
        {
            throw new NotImplementedException();
        }
    }
}
