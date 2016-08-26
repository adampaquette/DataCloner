using DataCloner.Core.Internal;
using System.Text;

namespace DataCloner.Core.Data.Generator
{
    public class UpdateWriter : IUpdateWriter
    {
        private string IdentifierDelemiterStart { get; }
        private string IdentifierDelemiterEnd { get; }

        private readonly StringBuilder _sbSet = new StringBuilder();
        private readonly StringBuilder _sbWhere = new StringBuilder();

        public UpdateWriter(UpdateStep step, string identifierDelemiterStart, string identifierDelemiterEnd)
        {
            IdentifierDelemiterStart = identifierDelemiterStart;
            IdentifierDelemiterEnd = identifierDelemiterEnd;

            _sbSet.Append("UPDATE ")
               .Append(step.DestinationRow.Database)
               .Append(".")
               .Append(step.DestinationRow.Table)
               .Append(" SET ");
        }

        public IUpdateWriter AppendToSet(string colName, string paramName)
        {
            _sbSet.Append(IdentifierDelemiterStart).Append(colName).Append(IdentifierDelemiterEnd)
               .Append(" = ")
               .Append(paramName)
               .Append(",");
            return this;
        }

        public IUpdateWriter AppendToWhere(string colName, string paramName)
        {
            _sbWhere.Append(_sbWhere.Length == 0 ? " WHERE " : " AND ");
            _sbWhere.Append(IdentifierDelemiterStart).Append(colName).Append(IdentifierDelemiterEnd).Append(" = ").Append(paramName);
            return this;
        }

        public IUpdateWriter Complete()
        {
            _sbWhere.Append(";\r\n");
            return this;
        }

        public StringBuilder ToStringBuilder()
        {
            var sb = new StringBuilder();
            sb.Append(_sbSet);

            if (_sbWhere.Length != 0)
                sb.Append(_sbWhere);

            return sb;
        }
    }
}
