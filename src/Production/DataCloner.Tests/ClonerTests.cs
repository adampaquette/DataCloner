using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataCloner.DataAccess;
using DataCloner.DataClasse;
using DataCloner.DataClasse.Cache;
using DataCloner.DataClasse.Configuration;
using Xunit;
using NSubstitute;
using NSubstitute.Core;

namespace DataCloner.Tests
{
    public class ClonerTests
    {
        private readonly Cache _cache;
        private readonly Cloner _cloner;
        private readonly IQueryHelper _queryHelper;
        private readonly IQueryDispatcher _queryDispatcher;

        public ClonerTests()
        {
            _cache = CreateCache();

            _queryHelper = Substitute.For<IQueryHelper>();
            _queryHelper.Select(NewRi(0, "", "", "customer", new ColumnsWithValue { { "id", 1 } }))
                        .Returns(new[] { new object[] { 1, 2, 3 } });

            _queryDispatcher = Substitute.For<IQueryDispatcher>();
            _queryDispatcher.GetQueryHelper(0).Returns(_queryHelper);
            _queryDispatcher.GetQueryHelper(Arg.Is<IRowIdentifier>(r => r.ServerId == 0)).Returns(_queryHelper);

            _cloner = new Cloner(_queryDispatcher, (a, b, c, d, e, f) => _cache);
            _cloner.Config = new Configuration();
        }

        public static IRowIdentifier NewRi(Int16 serverId, string database,
            string schema, string table, ColumnsWithValue cols)
        {
            return new RowIdentifier
            {
                ServerId = serverId,
                Database = database,
                Schema = schema,
                Table = table,
                Columns = cols
            };
        }


        [Fact]
        public void QueryDispatcherCalledWithRowIdentifierFromExtensionReturnData()
        {
            var row = NewRi(0, "", "", "customer", new ColumnsWithValue { { "id", 1 } });
            var result = _queryDispatcher.Select(row);

            _queryDispatcher.Received().GetQueryHelper(row);
            _queryHelper.Received().Select(row);

            Assert.Equal(new[] { new object[] { 1, 2, 3 } }, result);
        }

        [Fact]
        public void QueryDispatcherCalledWithRowIdentifierReturnData()
        {
            var row = NewRi(0, "", "", "customer", new ColumnsWithValue { { "id", 1 } });
            var result = _queryDispatcher.GetQueryHelper(row).Select(row);

            _queryDispatcher.Received().GetQueryHelper(row);
            _queryHelper.Received().Select(row);

            Assert.Equal(new[] { new object[] { 1, 2, 3 } }, result);
        }

        [Fact]
        public void QueryDispatcherCalledWithIntegerReturnGoodData()
        {
            var row = NewRi(0, "", "", "customer", new ColumnsWithValue { { "id", 1 } });
            var result = _queryDispatcher.GetQueryHelper(0).Select(row);

            _queryDispatcher.Received().GetQueryHelper(row.ServerId);
            _queryHelper.Received().Select(row);

            Assert.Equal(new[] { new object[] { 1, 2, 3 } }, result);
        }

        [Fact]
        public void BasicTest()
        {
            var source = NewRi(0, "", "", "customer", new ColumnsWithValue { { "id", 1 } });
            _cloner.Clone(null, null, null, null, source, true);

            _queryDispatcher.Received();
            _queryHelper.Received();
            _queryHelper.Received().Select(Arg.Any<IRowIdentifier>());
            _queryHelper.Received().Select(NewRi(0, "", "", "customer", new ColumnsWithValue { { "id", 1 } }));
        }

        private static Cache CreateCache()
        {
            var customerTable = new TableSchema();
            customerTable.Name = "customer";
            customerTable.ColumnsDefinition = new[]
            {
                new ColumnDefinition {Name = "pk", IsPrimary = true, Type = DbType.Int32},
                new ColumnDefinition {Name = "name", Type = DbType.String, Size = "20"}
            };

            var cache = new Cache();
            cache.ServerMap.Add(new ServerIdentifier { Database = "", Schema = "" },
                                new ServerIdentifier { Database = "", Schema = "" });
            cache.DatabasesSchema.Add(0, "", "", customerTable);
            return cache;
        }
    }
}
