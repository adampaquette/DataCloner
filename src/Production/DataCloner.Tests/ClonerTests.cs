using System;
using DataCloner.Data;
using DataCloner.Internal;
using DataCloner.Metadata;
using DataCloner.Configuration;
using NSubstitute;
using Xunit;

namespace DataCloner.Tests
{
	public class ClonerTests
	{
		public class BasicClonerTests
		{
			private readonly Metadata.MetadataContainer _cache;
			private readonly Cloner _cloner;
			private readonly IQueryHelper _queryHelper;
			private readonly IQueryDispatcher _queryDispatcher;

			public BasicClonerTests()
			{
				_cache = FakeBasicDatabase.CreateDatabaseSchema();
				_queryHelper = FakeBasicDatabase.CreateData();
				_queryDispatcher = FakeBasicDatabase.CreateServer(_queryHelper);

                _cloner = new Cloner(_queryDispatcher, (IQueryDispatcher d, Settings s, ref Metadata.MetadataContainer m) => m = _cache);
				_cloner.Setup(null);
			}

			#region QueryDispatcher

			[Fact]
			public void QueryDispatcher_Select_CalledWithRowIdentifierReturnData()
			{
				var row = Make.Ri0("color", new ColumnsWithValue { { "id", 1 } });
				var result = _queryDispatcher.Select(row);

				_queryDispatcher.Received().GetQueryHelper(row);
				_queryHelper.Received().Select(row);

				Assert.Equal(Make.Obj(1, "orange"), result);
			}

			[Fact]
			public void QueryDispatcher_GetQueryHelper_CalledWithRowIdentifierReturnData()
			{
				var row = Make.Ri0("color", new ColumnsWithValue { { "id", 1 } });
				var result = _queryDispatcher.GetQueryHelper(row).Select(row);

				_queryDispatcher.Received().GetQueryHelper(row);
				_queryHelper.Received().Select(row);

				Assert.Equal(Make.Obj(1, "orange"), result);
			}

			[Fact]
			public void QueryDispatcher_GetQueryHelper_CalledWithIntegerReturnData()
			{
				var row = Make.Ri0("color", new ColumnsWithValue { { "id", 1 } });
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
				var row = Make.Ri0("color", new ColumnsWithValue { { "id", id } });
				var clones = _cloner.Clone(row, true);

				Assert.Equal(0, clones.Count);
			}

			[Fact]
			public void Cloner_Clone_Param_ShouldThrow()
			{
				Assert.Throws(typeof(ArgumentNullException), () => _cloner.Clone(null, true));
			}

			[Fact]
			public void Cloner_Clone_OneRowOneTable()
			{
				var row = Make.Ri0("color", new ColumnsWithValue { { "id", 1 } });
				var clones = _cloner.Clone(row, true);

				Assert.Equal(1, clones.Count);
				Assert.Equal("color", clones[0].Table);
				Assert.IsType<SqlVariable>(clones[0].Columns["id"]);
			}

			[Fact]
			public void Cloner_Clone_RecursiveDontCrash()
			{
				var row = Make.Ri0("person", new ColumnsWithValue { { "id", 1 } });
				var clones = _cloner.Clone(row, true);

				Assert.Equal(1, clones.Count);
				Assert.Equal("person", clones[0].Table);
				Assert.IsType<SqlVariable>(clones[0].Columns["id"]);
			}

			#endregion
		}
	}
}
