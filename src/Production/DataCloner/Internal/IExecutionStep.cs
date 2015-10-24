using System;

namespace DataCloner.Internal
{
	public interface IExecutionStep
	{
		Int32 StepId { get; set; }
		ITableIdentifier DestinationTable { get; set; }
	}
}
