using System;
using System.Data;
using DataCloner.Internal;
using DataCloner.Metadata;

namespace DataCloner.Tests
{
    internal static class Make
    {
        internal static IRowIdentifier Ri(Int16 serverId, string database,
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

		internal static IRowIdentifier Ri0(string table, ColumnsWithValue cols)
		{
			return new RowIdentifier
			{
				ServerId = 0,
				Database = "",
				Schema = "",
				Table = table,
				Columns = cols
			};
		}

		internal static object[][] Obj(params object[] param)
		{
			return new object[][] {param};
		}

		internal static ColumnsWithValue[][] Col(params ColumnsWithValue[] param)
		{
			return new ColumnsWithValue[][] { param };
		}
	}
}