namespace DataCloner.Core.Internal
{
    public abstract class ExecutionStep
    {
        public int StepId { get; set; }
        public TableIdentifier DestinationTable { get; set; }
    }
}

