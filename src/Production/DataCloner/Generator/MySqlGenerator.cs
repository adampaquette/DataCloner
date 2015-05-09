namespace DataCloner.Generator
{
    internal class MySqlGenerator : AbstractSqlGenerator
    {
        public override char DelemitedIdentifierCaracter => '`';
    }
}
