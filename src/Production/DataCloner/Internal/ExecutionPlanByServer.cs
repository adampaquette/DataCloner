using DataCloner.Archive;
using DataCloner.Framework;
using System;
using System.Collections.Generic;
using System.IO;

namespace DataCloner.Internal
{
    public class ExecutionPlanByServer : Dictionary<Int16, ExecutionPlan>
    {
        public void Serialize(Stream output, DecompresibleList referenceTracking)
        {
            using (var br = new BinaryWriter(output))
                Serialize(br, referenceTracking);
        }

        public void Serialize(BinaryWriter output, DecompresibleList referenceTracking)
        {
            output.Write(Count);
            foreach (var srv in this)
            {
                output.Write(srv.Key);
                srv.Value.Serialize(output, referenceTracking);
            }
        }


        public static ExecutionPlan Deserialize(Stream stream)
        {

            return null;
        }
    }
}
