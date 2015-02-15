using DataCloner.DataAccess;
using DataCloner.DataClasse;
using DataCloner.DataClasse.Cache;
using DataCloner.DataClasse.Configuration;
using DataCloner.Tests.Tools;
using NSubstitute;
using Xunit;

namespace DataCloner.Tests
{
    public class ClonerTests
    {
        public class BasicClonerTests
        {
            private readonly Cache _cache;
            private readonly Cloner _cloner;
            private readonly IQueryHelper _queryHelper;
            private readonly IQueryDispatcher _queryDispatcher;

            public BasicClonerTests()
            {
                _cache = FakeBasicDatabase.CreateCache();
                _queryHelper = FakeBasicDatabase.CreateData();

                _queryDispatcher = Substitute.For<IQueryDispatcher>();
                _queryDispatcher.GetQueryHelper(0).Returns(_queryHelper);
                _queryDispatcher.GetQueryHelper(Arg.Is<IRowIdentifier>(r => r.ServerId == 0)).Returns(_queryHelper);

                _cloner = new Cloner(_queryDispatcher, (a, b, c, d, e, f) => _cache);
                _cloner.Config = new Configuration();
            }

            [Fact]
            public void QueryDispatcherCalledWithRowIdentifierFromExtensionReturnData()
            {
                var row = NewRi(0, "", "", "customer", new ColumnsWithValue {{"id", 1}});
                var result = _queryDispatcher.Select(row);

                _queryDispatcher.Received().GetQueryHelper(row);
                _queryHelper.Received().Select(row);

                Assert.Equal(new[] {new object[] {1, 2, 3}}, result);
            }

            [Fact]
            public void QueryDispatcherCalledWithRowIdentifierReturnData()
            {
                var row = NewRi(0, "", "", "customer", new ColumnsWithValue {{"id", 1}});
                var result = _queryDispatcher.GetQueryHelper(row).Select(row);

                _queryDispatcher.Received().GetQueryHelper(row);
                _queryHelper.Received().Select(row);

                Assert.Equal(new[] {new object[] {1, 2, 3}}, result);
            }

            [Fact]
            public void QueryDispatcherCalledWithIntegerReturnData()
            {
                var row = NewRi(0, "", "", "customer", new ColumnsWithValue {{"id", 1}});
                var result = _queryDispatcher.GetQueryHelper(0).Select(row);

                _queryDispatcher.Received().GetQueryHelper(row.ServerId);
                _queryHelper.Received().Select(row);

                Assert.Equal(new[] {new object[] {1, 2, 3}}, result);
            }

            [Fact]
            public void BasicTest()
            {
                var source = NewRi(0, "", "", "customer", new ColumnsWithValue {{"id", 1}});
                _cloner.Clone(null, null, null, null, source, true);

                _queryDispatcher.Received();
                _queryHelper.Received();
                _queryHelper.Received().Select(Arg.Any<IRowIdentifier>());
                _queryHelper.Received().Select(NewRi(0, "", "", "customer", new ColumnsWithValue {{"id", 1}}));
            }
        }
    }
}
