using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace DataCloner.Internal
{
	public class UpdateStep : ExecutionStep
	{
		public RowIdentifier DestinationRow { get; set; }
		public ColumnsWithValue ForeignKey { get; set; }

        public void Serialize(BinaryWriter output)
        {
            var bf = new BinaryFormatter();

            output.Write(StepId);
            DestinationTable.Serialize(output.BaseStream);
            DestinationRow.Serialize(output.BaseStream);
            bf.Serialize(output.BaseStream, ForeignKey);
        }
    }
}