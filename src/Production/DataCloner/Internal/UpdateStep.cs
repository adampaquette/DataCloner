using DataCloner.Framework;
using System.IO;

namespace DataCloner.Internal
{
    public class UpdateStep : ExecutionStep
	{
		public RowIdentifier DestinationRow { get; set; }
		public ColumnsWithValue ForeignKey { get; set; }

        public void Serialize(BinaryWriter output)
        {
            var bf = SerializationHelper.DefaultFormatter;

            output.Write(StepId);
            DestinationTable.Serialize(output);
            DestinationRow.Serialize(output);
            bf.Serialize(output.BaseStream, ForeignKey);
        }

        public static UpdateStep Deserialize(BinaryReader input)
        {
            var step = new UpdateStep();
            var bf = SerializationHelper.DefaultFormatter;

            step.StepId = input.ReadInt32();
            step.DestinationTable = TableIdentifier.Deserialize(input);
            step.DestinationRow = RowIdentifier.Deserialize(input);
            step.ForeignKey = ColumnsWithValue.Deserialize(input);

            return step;
        }
    }
}