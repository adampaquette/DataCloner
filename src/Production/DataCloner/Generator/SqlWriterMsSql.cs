namespace DataCloner.Generator
{
    internal class SqlWriterMsSql : AbstractSqlWriter
    {
        private readonly static IInsertWriter _insertWriter = new InsertWriterMsSql();

        public override IInsertWriter GetInsertWriter()
        {
            return _insertWriter;
        }
    }
}
