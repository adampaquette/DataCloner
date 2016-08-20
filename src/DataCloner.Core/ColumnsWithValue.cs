using DataCloner.Core.Framework;
using System;
using System.Collections.Generic;
using System.IO;

namespace DataCloner.Core
{
    public class ColumnsWithValue : Dictionary<string, object>
    {
        public ColumnsWithValue() : base(StringComparer.OrdinalIgnoreCase) { }

        public void Serialize(BinaryWriter output)
        {
            output.Write(Count);
            foreach (var col in this)
            {
                output.Write(col.Key);
                SerializationHelper.Serialize(output.BaseStream, col.Value);
            }
        }

        public static ColumnsWithValue Deserialize(BinaryReader input)
        {
            var cols = new ColumnsWithValue();

            var nbCols = input.ReadInt32();
            for (int i = 0; i < nbCols; i++)
            {
                var key = input.ReadString();
                var value = SerializationHelper.Deserialize<Object>(input.BaseStream);
                cols.Add(key, value);
            }
            return cols;
        }
    }
}
