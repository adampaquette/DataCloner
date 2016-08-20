using DataCloner.Core.Framework;
using System;
using System.Collections.Generic;
using System.IO;

namespace DataCloner.Core.Internal
{
    public class ExecutionPlanByServer : Dictionary<Int16, ExecutionPlan>
    {
        public void Serialize(Stream output, FastAccessList<object> referenceTracking)
        {
            using (var br = new BinaryWriter(output, System.Text.Encoding.UTF8, true))
                Serialize(br, referenceTracking);
        }

        public void Serialize(BinaryWriter output, FastAccessList<object> referenceTracking)
        {
            output.Write(Count);
            foreach (var srv in this)
            {
                output.Write(srv.Key);
                srv.Value.Serialize(output, referenceTracking);
            }
        }


        public static ExecutionPlanByServer Deserialize(BinaryReader input, FastAccessList<object> referenceTracking)
        {
            var epBySrv = new ExecutionPlanByServer();

            var nbSrv = input.ReadInt32();
            for (int i = 0; i < nbSrv; i++)
            {
                var key = input.ReadInt16();
                var value = ExecutionPlan.Deserialize(input, referenceTracking);
                epBySrv.Add(key, value);
            }

            return epBySrv;
        }
    }
}
