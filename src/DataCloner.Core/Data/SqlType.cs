using System.IO;

namespace DataCloner.Core.Data
{
    public class SqlType
    {
        public string DataType { get; set; }
        public int Precision { get; set; }
        public int Scale { get; set; }
        public bool IsUnsigned { get; set; }

        internal void Serialize(BinaryWriter stream)
        {
            stream.Write(DataType ?? "");
            stream.Write(Precision);
            stream.Write(Scale);
            stream.Write(IsUnsigned);
        }

        internal static SqlType Deserialize(BinaryReader stream)
        {
            return new SqlType
            {
                DataType = stream.ReadString(),
                Precision = stream.ReadInt32(),
                Scale = stream.ReadInt32(),
                IsUnsigned = stream.ReadBoolean()
            };
        }
    }
}
