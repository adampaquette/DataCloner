namespace DataCloner.Core.Data.Generator
{
    public abstract class AbstractSqlWriter : ISqlWriter
    {
        public abstract IInsertWriter GetInsertWriter();
    }
}
