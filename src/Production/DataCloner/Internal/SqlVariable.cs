using System;
using System.Diagnostics;

namespace DataCloner.Internal
{
    [DebuggerDisplay("SqlVar: Key={Id}, Value={Value}")]
    public class SqlVariable : IEquatable<SqlVariable>
    {
        private Int32 _id;

        public Int32 Id
        {
            get { return _id; }
        }
        public object Value { get; set; }
        public bool QueryValue { get; set; }

        public SqlVariable(Int32 id)
        {
            _id = id;
            QueryValue = true;
        }

        public override bool Equals(object obj)
        {
            var sv = obj as SqlVariable;
            if (sv != null)
                return sv.Id == Id;
            return false;
        }

        public bool Equals(SqlVariable other)
        {
            if (other != null)
                return other.Id == Id;
            return false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
