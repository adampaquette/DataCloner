using System;
using System.Collections.Generic;
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
        public ClonerTests()
        {

        }

        [Fact]
        public void BasicTest()
        {
            var cache = new Cache();
            cache.ServerMap.Add(new ServerIdentifier { ServerId = 1, Database = "bd", Schema = ""},
                                new ServerIdentifier { ServerId = 1, Database = "bd", Schema = ""});

            var queryHelper = Substitute.For<IQueryHelper>();
            queryHelper.Select(new RowIdentifier{ServerId = 1, Database = "bd", Schema = "", Table = "tbl1",
                Columns = new ColumnsWithValue { { "pk", 1 } }})
                .Returns(new[] { new object[] {1,1} });

            var dispatcher = Substitute.For<IQueryDispatcher>();
            dispatcher.GetQueryHelper(1).Returns(new QueryHelperMySql(cache, null, 1));

            dispatcher.GetQueryHelper(1);
            dispatcher.Received().GetQueryHelper(1);

            var dc = new Cloner(dispatcher)
            {
                Config = null,
                EnforceIntegrity = false
            };
            dc.Logger += Console.WriteLine;

            var source = new RowIdentifier();
            source.Columns.Clear();
            source.ServerId = 1;
            source.Database = "sakila";
            source.Schema = "";
            source.Table = "customer";
            source.Columns.Add("customer_id", 1);
            dc.Clone("TestApp", "testMySQL", "testMySQL", null, source, true);

        }
    }
}
