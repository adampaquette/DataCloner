using DataCloner.Core.Metadata;
using System.Collections.Generic;
using System.Text;

namespace DataCloner.Core.Data.Generator
{
    public class InsertWriter : IInsertWriter
    {
        private string IdentifierDelemiterStart { get; }
        private string IdentifierDelemiterEnd { get; }
        private string StringDelemiter { get; }
        private string NamedParameterPrefix { get; }

        private readonly StringBuilder _sb = new StringBuilder();

        public InsertWriter(string identifierDelemiterStart, string identifierDelemiterEnd , string stringDelemiter, string namedParameterPrefix)
        {
            IdentifierDelemiterStart = identifierDelemiterStart;
            IdentifierDelemiterEnd = identifierDelemiterEnd;
            StringDelemiter = stringDelemiter;
            NamedParameterPrefix = namedParameterPrefix;
        }

        public IInsertWriter AppendColumns(TableIdentifier table, List<ColumnDefinition> columns)
        {
            _sb.Append("INSERT INTO ")
               .Append(IdentifierDelemiterStart).Append(table.Database).Append(IdentifierDelemiterEnd).Append(".");

            if (!string.IsNullOrWhiteSpace(table.Schema))
                _sb.Append(IdentifierDelemiterStart).Append(table.Schema).Append(IdentifierDelemiterEnd).Append(".");
            _sb.Append(IdentifierDelemiterStart).Append(table.Table).Append(IdentifierDelemiterEnd)
               .Append("(");

            //Nom des colonnes
            foreach (var column in columns)
            {
                if (!column.IsAutoIncrement)
                    _sb.Append(IdentifierDelemiterStart).Append(column.Name).Append(IdentifierDelemiterEnd).Append(",");
            }
            _sb.Remove(_sb.Length - 1, 1);
            _sb.Append(")VALUES(");

            return this;
        }

        public IInsertWriter Append(string value)
        {
            _sb.Append(value);
            return this;
        }

        public IInsertWriter AppendValue(object value)
        {
            _sb.Append(StringDelemiter).Append(value).Append(StringDelemiter).Append(",");
            return this;
        }

        public IInsertWriter AppendVariable(string varName)
        {
            _sb.Append(NamedParameterPrefix).Append(varName).Append(",");
            return this;
        }

        public IInsertWriter Complete()
        {
            _sb.Remove(_sb.Length - 1, 1);
            _sb.Append(");\r\n");
            return this;
        }

        public StringBuilder ToStringBuilder()
        {
            return _sb;
        }
    }
}
