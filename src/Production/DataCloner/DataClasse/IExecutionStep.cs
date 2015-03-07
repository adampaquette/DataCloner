using System;

namespace DataCloner.DataClasse
{
	public interface IExecutionStep
	{
		Int32 StepId { get; set; }
		ITableIdentifier DestinationTable { get; set; }
	}
}
