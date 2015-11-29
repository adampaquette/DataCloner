using System;
using System.Collections.Generic;
using DataCloner.Metadata;
using System.Diagnostics;
using System.IO;
using DataCloner.Archive;
using DataCloner.Framework;
using System.Runtime.Serialization.Formatters.Binary;

namespace DataCloner.Internal
{
    [DebuggerDisplay("{SourceTable.ServerId.ToString() + \".\" + SourceTable.Database + \".\" + SourceTable.Schema + \".\" + SourceTable.Table}...")]
	public class InsertStep : ExecutionStep
    {
		public Int32 Depth { get; set; }
		public List<SqlVariable> Variables { get; set; }
		public TableMetadata TableSchema { get; set; }
        public TableIdentifier SourceTable { get; set; }
        public object[] Datarow { get; set; }

		public InsertStep()
		{
			Variables = new List<SqlVariable>();
		}

        public void Serialize(BinaryWriter output, DecompresibleList referenceTracking)
        {
            var bf = new BinaryFormatter();

            output.Write(StepId);
            output.Write(Depth);
            SourceTable.Serialize(output.BaseStream);
            DestinationTable.Serialize(output.BaseStream);

            //Variable
            output.Write(Variables.Count);
            foreach (var v in Variables)
            {
                var vId = referenceTracking.TryAdd(v);
                output.Write(vId);
            }

            //TableSchema
            var tsId = referenceTracking.TryAdd(TableSchema);
            output.Write(tsId);

            //Datarow
            output.Write(Datarow.Length);
            for(var i=0; i<Datarow.Length; i++)
            {
                var data = Datarow[i];
                var type = data.GetType();
                var tag = SerializationHelper.TypeToTag[type];
                output.Write(tag);

                if (type == typeof(SqlVariable))
                {
                    var svId = referenceTracking.TryAdd(data);
                    output.Write(svId);
                }
                else
                    bf.Serialize(output.BaseStream, data);
            }
        }

        public static InsertStep Deserialize(BinaryReader input, DecompresibleList referenceTracking)
        {
            var bf = new BinaryFormatter();
            var step = new InsertStep();

            step.StepId = input.ReadInt32();
            step.Depth = input.ReadInt32();
            step.SourceTable = TableIdentifier.Deserialize(input);
            step.DestinationTable = TableIdentifier.Deserialize(input);

            //Variable
            var nbVars = input.ReadInt32();
            for (int i = 0; i < nbVars; i++)
            {
                var id = input.ReadInt32();
                step.Variables.Add((SqlVariable)referenceTracking[id]);
            }

            //TableSchema
            var tsId = input.ReadInt32();
            step.TableSchema = (TableMetadata)referenceTracking[tsId];

            //Datarow
            var nbRows = input.ReadInt32();
            step.Datarow = new object[nbRows];
            for (var i = 0; i < nbRows; i++)
            {
                var data = step.Datarow[i];
                var tag = input.ReadInt32();
                var type = SerializationHelper.TagToType[tag];

                if (type == typeof(SqlVariable))
                {
                    var id = input.ReadInt32();
                    data = referenceTracking[id];
                }
                else
                    data = bf.Deserialize(input.BaseStream);
            }

            return step;
        }
    }
}
