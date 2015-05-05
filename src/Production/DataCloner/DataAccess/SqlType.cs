using System;

namespace DataCloner.DataAccess
{
    public class SqlType
    {
        public string DataType { get; set; }
        public Int32 Precision { get; set; }
        public Int32 Scale { get; set; }
        public bool IsUnsigned { get; set; }
    }
}
