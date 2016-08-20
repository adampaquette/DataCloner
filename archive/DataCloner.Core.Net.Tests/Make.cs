using DataCloner.Core.Internal;

namespace DataCloner.Core.Tests
{
    internal static class Make
    {
		internal static RowIdentifier Row(string table, string colName, object colValue)
		{
			return new RowIdentifier
			{
				ServerId = 0,
				Database = "",
				Schema = "",
				Table = table,
				Columns = new ColumnsWithValue { { colName, colValue } }
            };
		}

		internal static object[][] Obj(params object[] param)
		{
			return new object[][] {param};
		}
	}
}