using DataCloner.Core.Metadata;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;

namespace DataCloner.Core.Data.Generator
{
    public class SelectWriter : ISelectWriter
    {
        private string IdentifierDelemiterStart { get; }
        private string IdentifierDelemiterEnd { get; }

        private readonly StringBuilder _sb = new StringBuilder();
        private readonly StringBuilder _sbWhere = new StringBuilder();

        public SelectWriter(string identifierDelemiterStart, string identifierDelemiterEnd)
        {
            IdentifierDelemiterStart = identifierDelemiterStart;
            IdentifierDelemiterEnd = identifierDelemiterEnd;
        }

        public ISelectWriter AppendColumns(RowIdentifier row, List<ColumnDefinition> columns)
        {
            _sb.Append("SELECT ");

            //Nom des colonnes
            for (var i = 0; i < columns.Count(); i++)
                _sb.Append(IdentifierDelemiterStart).Append(columns[i].Name).Append(IdentifierDelemiterEnd).Append(",");
            _sb.Remove(_sb.Length - 1, 1)
               .Append(" FROM ")
               .Append(IdentifierDelemiterStart).Append(row.Database).Append(IdentifierDelemiterEnd).Append(".");
            if (!String.IsNullOrWhiteSpace(row.Schema))
                _sb.Append(IdentifierDelemiterStart).Append(row.Schema).Append(IdentifierDelemiterEnd).Append(".");
            _sb.Append(IdentifierDelemiterStart).Append(row.Table).Append(IdentifierDelemiterEnd);

            return this;
        }

        public ISelectWriter AppendToWhere(string colName, string paramName)
        {
            _sbWhere.Append(_sbWhere.Length == 0 ? " WHERE " : " AND ");
            _sbWhere.Append(IdentifierDelemiterStart).Append(colName).Append(IdentifierDelemiterEnd).Append(" = ").Append(paramName);

            return this;
        }

        public StringBuilder ToStringBuilder()
        {
            var sb = new StringBuilder();
            sb.Append(_sb);

            if (_sbWhere.Length != 0)
                sb.Append(_sbWhere);

            return sb;
        }
    }
}