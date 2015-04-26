using System;

namespace DataCloner.DataClasse
{
    public class SqlVariable : IEquatable<SqlVariable>
    {
        public Int32 Id { get; set; }
        public object Value { get; set; }
		public int ReferenceCount { get; set; }

		public override bool Equals(object obj)
		{
			if (obj == null) return false;

			var sv = obj as SqlVariable;
			if (sv == null) return false;

			return sv.Id == Id;
		}

		public bool Equals(SqlVariable other)
		{
			if (other == null) return false;
			return other.Id == Id;
		}

		public override int GetHashCode()
		{
			return Id.GetHashCode();
		}
	}
}
