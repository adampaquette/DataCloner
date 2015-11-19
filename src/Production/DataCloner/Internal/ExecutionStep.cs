using System;

namespace DataCloner.Internal
{
    public abstract class ExecutionStep
    {
        public Int32 StepId { get; set; }
        public TableIdentifier DestinationTable { get; set; }
    }
}

