namespace DataCloner.Core.Tests
{
    public class KeyRelationshipTests
    {
        private readonly KeyRelationship _keys;

        public KeyRelationshipTests()
        {
            _keys = new KeyRelationship();
            _keys.SetKey(1, "db", "dbo", "table1", new object[] { 1, 1 }, new object[] { 1, 2 });
            _keys.SetKey(1, "db", "dbo", "table1", new object[] { 1, 2 }, new object[] { 1 });
        }

        [Fact]
        public void Should_ReturnDestinationKey_When_SourceKeyIsSelected()
        {
            var key = _keys.GetKey(1, "db", "dbo", "table1", new object[] { 1, 1 });
            var key2 = _keys.GetKey(1, "db", "dbo", "table1", new object[] { 1, 2 });

            Assert.True(StructuralComparisons.StructuralEqualityComparer.Equals(new object[] { 1, 2 }, key));
            Assert.True(StructuralComparisons.StructuralEqualityComparer.Equals(new object[] { 1 }, key2));
        }      
    }
}
