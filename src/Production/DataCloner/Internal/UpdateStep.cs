namespace DataCloner.Internal
{
	public class UpdateStep : ExecutionStep
	{
		public RowIdentifier DestinationRow { get; set; }
		public ColumnsWithValue ForeignKey { get; set; }
	}
}