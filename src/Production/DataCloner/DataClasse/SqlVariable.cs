using System;

namespace DataCloner.DataClasse
{
    public class SqlVariable : IEquatable<SqlVariable>
    {
        public Int32 Id { get; }
        public object Value { get; set; }
		public bool QueryValue { get; set; }

        public SqlVariable(Int32 id)
        {
            Id = id;
            QueryValue = true;
        }

		public override bool Equals(object obj)
		{
		    var sv = obj as SqlVariable;
		    return sv?.Id == Id;
		}
    
        public bool Equals(SqlVariable other)
		{
            return other?.Id == Id;
		}

		public override int GetHashCode()
		{
			return Id.GetHashCode();
		}
	}
}
