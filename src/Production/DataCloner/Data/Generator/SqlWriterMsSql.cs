namespace DataCloner.Core.Data.Generator
{
    internal class SqlWriterMsSql : AbstractSqlWriter
    {
        public override IInsertWriter GetInsertWriter()
        {
            return new InsertWriterMsSql();
        }
    }
}