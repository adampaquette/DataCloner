using System;
using System.Collections.Generic;
using DataCloner.Metadata;
using System.Diagnostics;

namespace DataCloner.Internal
{
    [DebuggerDisplay("{SourceTable.ServerId.ToString() + \".\" + SourceTable.Database + \".\" + SourceTable.Schema + \".\" + SourceTable.Table}...")]
	public class InsertStep : ExecutionStep
    {
		public Int32 Depth { get; set; }
		public List<SqlVariable> Variables { get; set; }
		public TableMetadata TableSchema { get; set; }
        public TableIdentifier SourceTable { get; set; }
        public object[] DataRow { get; set; }

		public InsertStep()
		{
			Variables = new List<SqlVariable>();
		}
	}
}
