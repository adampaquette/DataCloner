using DataCloner.Data;
using DataCloner.Metadata;
using NSubstitute;
using System;
using Xunit;

namespace DataCloner.Tests
{
    public class ClonerTests
	{
		public class BasicClonerTests
		{
			private readonly MetadataContainer _cache;
			private readonly ExecutionPlanBuilder _executionPlanBuilder;
			private readonly IQueryHelper _queryHelper;
			private readonly IQueryDispatcher _queryDispatcher;

			public BasicClonerTests()
			{
				_cache = FakeBasicDatabase.CreateDatabaseSchema();
				_queryHelper = FakeBasicDatabase.CreateData();
				_queryDispatcher = FakeBasicDatabase.CreateServer(_queryHelper);

                _executionPlanBuilder = new ExecutionPlanBuilder(null, _queryDispatcher, 
                    (IQueryDispatcher d, Settings s, ref MetadataContainer m) => m = _cache, null);
			}

			#region QueryDispatcher

			[Fact]
			public void QueryDispatcher_Select_CalledWithRowIdentifierReturnData()
			{
				var row = Make.Row("color", "id", 1);
				var result = _queryDispatcher.Select(row);

				_queryDispatcher.Received().GetQueryHelper(row);
				_queryHelper.Received().Select(row);

				Assert.Equal(Make.Obj(1, "orange"), result);
			}

			[Fact]
			public void QueryDispatcher_GetQueryHelper_CalledWithRowIdentifierReturnData()
			{
				var row = Make.Row("color", "id", 1 );
				var result = _queryDispatcher.GetQueryHelper(row).Select(row);

				_queryDispatcher.Received().GetQueryHelper(row);
				_queryHelper.Received().Select(row);

				Assert.Equal(Make.Obj(1, "orange"), result);
			}

			[Fact]
			public void QueryDispatcher_GetQueryHelper_CalledWithIntegerReturnData()
			{
				var row = Make.Row("color", "id", 1);
				var result = _queryDispatcher.GetQueryHelper(0).Select(row);

				_queryDispatcher.Received().GetQueryHelper(row.ServerId);
				_queryHelper.Received().Select(row);

				Assert.Equal(Make.Obj(1, "orange"), result);
			}

			#endregion

			#region Cloner

			[Theory]
			[InlineData(int.MinValue)]
			[InlineData(int.MaxValue)]
			public void Cloner_Clone_Param_ReturnNoRow(int id)
			{
				var row = Make.Row("color",  "id", id);
				var clones = _executionPlanBuilder.Append(row, true).Compile().Execute();

				Assert.Equal(0, clones.Results.Count);
			}

			[Fact]
			public void Cloner_Clone_Param_ShouldThrow()
			{
				Assert.Throws(typeof(ArgumentNullException), () => _executionPlanBuilder.Append(null, true));
			}

			[Fact]
			public void Cloner_Clone_OneRowOneTable()
			{
				//var row = Make.Row("color", "id", 1 );
				//var clones = _executionPlanBuilder.Append(row, true).Compile().Execute();

				//Assert.Equal(1, clones.Results.Count);
				//Assert.Equal("color", clones.Results[0].Table);
				//Assert.Equal(null, clones.Results[0].Columns["id"]);
			}

			[Fact]
			public void Cloner_Clone_RecursiveDontCrash()
			{
				//var row = Make.Row("person", "id", 1);
				//var clones = _executionPlanBuilder.Append(row, true).Compile().Execute();

				//Assert.Equal(1, clones.Results.Count);
				//Assert.Equal("person", clones.Results[0].Table);
				//Assert.Equal(null, clones.Results[0].Columns["id"]);
			}

			#endregion
		}
	}
}
