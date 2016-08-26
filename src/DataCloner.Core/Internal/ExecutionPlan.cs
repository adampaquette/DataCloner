using DataCloner.Core.Framework;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DataCloner.Core.Internal
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

        public void Serialize(Stream output, FastAccessList<object> referenceTracking)
        {
            using (var bw = new BinaryWriter(output, Encoding.UTF8, true))
                Serialize(bw, referenceTracking);
        }

        public void Serialize(BinaryWriter output, FastAccessList<object> referenceTracking)
        {
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

        public static ExecutionPlan Deserialize(BinaryReader input, FastAccessList<object> referenceTracking)
        {
            var ep = new ExecutionPlan();

            var nbVars = input.ReadInt32();
            for (var i = 0; i < nbVars; i++)
            {
                var id = input.ReadInt32();
                ep.Variables.Add((SqlVariable)referenceTracking[id]);
            }

            var nbInsertSteps = input.ReadInt32();
            for (var i = 0; i < nbInsertSteps; i++)
            {
                var step = InsertStep.Deserialize(input, referenceTracking);
                ep.InsertSteps.Add(step);
            }

            var nbUpdateSteps = input.ReadInt32();
            for (var i = 0; i < nbUpdateSteps; i++)
            {
                var step = UpdateStep.Deserialize(input);
                ep.UpdateSteps.Add(step);
            }

            return ep;
        }
    }
}
