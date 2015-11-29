using DataCloner.Framework;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

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

        public void Serialize(Stream output, DecompresibleList referenceTracking)
        {
            using (var bw = new BinaryWriter(output))
                Serialize(bw, referenceTracking);
        }

        public void Serialize(BinaryWriter output, DecompresibleList referenceTracking)
        {
            var bf = new BinaryFormatter();

            output.Write(Variables.Count);
            foreach (var v in Variables)
            {
                var id = referenceTracking.TryAdd(v);
                output.Write(id);
            }

            output.Write(InsertSteps.Count);
            foreach (var step in InsertSteps)
                step.Serialize(output, referenceTracking);

            output.Write(UpdateSteps.Count);
            foreach (var step in UpdateSteps)
                step.Serialize(output);
        }

        public static ExecutionPlan Deserialize(Stream stream)
        {

            return null;
        }
    }
}
