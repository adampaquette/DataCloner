using System;

namespace DataCloner.Internal
{
	public class UpdateStep : IExecutionStep
	{
		public Int32 StepId { get; set; }
		public IRowIdentifier DestinationRow { get; set; }
		public ColumnsWithValue ForeignKey { get; set; }
		public ITableIdentifier DestinationTable { get; set; }
	}
}