using System.Collections.Generic;

namespace DataCloner.Internal
{
	public class ExecutionPlan 
	{
		public List<InsertStep> InsertSteps { get; set; }
		public List<UpdateStep> UpdateSteps { get; set; }
		public List<SqlVariable> Variables { get; set; }

		public ExecutionPlan()
		{
			InsertSteps = new List<InsertStep>();
			UpdateSteps = new List<UpdateStep>();
			Variables = new List<SqlVariable>();
		}

		public void Clear()
		{
			InsertSteps.Clear();
			UpdateSteps.Clear();
			Variables.Clear();
		}
	}
}
