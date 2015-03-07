using System.Collections.Generic;

namespace DataCloner.DataClasse
{
	public class ExecutionPlan 
	{
		public List<InsertStep> InsertSteps { get; set; }
		public List<UpdateStep> UpdateSteps { get; set; }

		public ExecutionPlan()
		{
			InsertSteps = new List<InsertStep>();
			UpdateSteps = new List<UpdateStep>();
		}

		public void Clear()
		{
			InsertSteps.Clear();
			UpdateSteps.Clear();
		}
	}
}
