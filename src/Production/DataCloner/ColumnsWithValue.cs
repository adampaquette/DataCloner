using DataCloner.Core.Framework;
using System.Collections.Generic;
using System.IO;

namespace DataCloner.Core
{
    public class ColumnsWithValue : Dictionary<string, object>
    {
        public void Serialize(BinaryWriter output)
        {
            var bf = SerializationHelper.DefaultFormatter;

            output.Write(Count);
            foreach (var col in this)
            {
                output.Write(col.Key);
                bf.Serialize(output.BaseStream, col.Value);
            }
        }

        public static ColumnsWithValue Deserialize(BinaryReader input)
        {
            var bf = SerializationHelper.DefaultFormatter;
            var cols = new ColumnsWithValue();

            var nbCols = input.ReadInt32();
            for (int i = 0; i < nbCols; i++)
            {
                var key = input.ReadString();
                var value = bf.Deserialize(input.BaseStream);
                cols.Add(key, value);
            }
            return cols;
        }
    }
}
