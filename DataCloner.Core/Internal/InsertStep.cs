using DataCloner.Core.Framework;
using DataCloner.Core.Metadata;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace DataCloner.Core.Internal
{
    [DebuggerDisplay("{SourceTable.ServerId.ToString() + \".\" + SourceTable.Database + \".\" + SourceTable.Schema + \".\" + SourceTable.Table}...")]
	public class InsertStep : ExecutionStep
    {
		public int Depth { get; set; }
		public List<SqlVariable> Variables { get; set; }
		public TableMetadata TableMetadata { get; set; }
        public TableIdentifier SourceTable { get; set; }
        public object[] Datarow { get; set; }

		public InsertStep()
		{
			Variables = new List<SqlVariable>();
		}

        public void Serialize(BinaryWriter output, FastAccessList<object> referenceTracking)
        {
            output.Write(StepId);
            output.Write(Depth);
            SourceTable.Serialize(output);
            DestinationTable.Serialize(output);

            //Variable
            output.Write(Variables.Count);
            foreach (var v in Variables)
            {
                var vId = referenceTracking.TryAdd(v);
                output.Write(vId);
            }

            //TableSchema
            var tsId = referenceTracking.TryAdd(TableMetadata);
            output.Write(tsId);

            //Datarow
            output.Write(Datarow.Length);
            foreach (var data in Datarow)
            {
                var type = data.GetType();
                var tag = SerializationHelper.TypeToTag[type];
                output.Write(tag);

                if (type == typeof(SqlVariable))
                {
                    var svId = referenceTracking.TryAdd(data);
                    output.Write(svId);
                }
                else
                    SerializationHelper.Serialize(output.BaseStream, data);
            }
        }

        public static InsertStep Deserialize(BinaryReader input, FastAccessList<object> referenceTracking)
        {
            var step = new InsertStep
            {
                StepId = input.ReadInt32(),
                Depth = input.ReadInt32(),
                SourceTable = TableIdentifier.Deserialize(input),
                DestinationTable = TableIdentifier.Deserialize(input)
            };

            //Variable
            var nbVars = input.ReadInt32();
            for (var i = 0; i < nbVars; i++)
            {
                var id = input.ReadInt32();
                step.Variables.Add((SqlVariable)referenceTracking[id]);
            }

            //TableSchema
            var tsId = input.ReadInt32();
            step.TableMetadata = (TableMetadata)referenceTracking[tsId];

            //Datarow
            var nbRows = input.ReadInt32();
            step.Datarow = new object[nbRows];
            for (var i = 0; i < nbRows; i++)
            {
                var tag = input.ReadInt32();
                var type = SerializationHelper.TagToType[tag];

                if (type == typeof(SqlVariable))
                {
                    var id = input.ReadInt32();
                    step.Datarow[i] = referenceTracking[id];
                }
                else
                    step.Datarow[i] = SerializationHelper.Deserialize<object>(input.BaseStream);
            }

            return step;
        }
    }
}
