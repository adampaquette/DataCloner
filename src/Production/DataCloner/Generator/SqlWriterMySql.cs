using System;

namespace DataCloner.Generator
{
    internal class SqlWriterMySql : AbstractSqlWriter
    {
        public override char DelemitedIdentifierCaracter => '`';

        public override IInsertWriter GetInsertWriter()
        {
            throw new NotImplementedException();
        }

    }
}
