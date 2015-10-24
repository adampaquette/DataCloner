using System;
using System.Collections.Generic;
using DataCloner.Metadata;
using System.Diagnostics;

namespace DataCloner.Internal
{
    [DebuggerDisplay("{SourceTable.ServerId.ToString() + \".\" + SourceTable.Database + \".\" + SourceTable.Schema + \".\" + SourceTable.Table}...")]
	public class InsertStep : IExecutionStep
    {
        public Int32 StepId { get; set; }
		public Int32 Depth { get; set; }
		public List<SqlVariable> Variables { get; set; }
		public ITableMetadata TableSchema { get; set; }
        public ITableIdentifier SourceTable { get; set; }
        public ITableIdentifier DestinationTable { get; set; }
        public object[] DataRow { get; set; }

		public InsertStep()
		{
			Variables = new List<SqlVariable>();
		}
	}
}
