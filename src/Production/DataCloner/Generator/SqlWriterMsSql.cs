namespace DataCloner.Generator
{
    internal class SqlWriterMsSql : AbstractSqlWriter
    {
        public override IInsertWriter GetInsertWriter()
        {
            return new InsertWriterMsSql();
        }
    }
}