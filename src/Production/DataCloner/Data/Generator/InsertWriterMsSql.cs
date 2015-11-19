using System.Linq;
using System.Text;
using DataCloner.Internal;
using DataCloner.Metadata;
using System.Collections.Generic;

namespace DataCloner.Data.Generator
{
    internal class InsertWriterMsSql : IInsertWriter
    {
        private readonly StringBuilder _sb = new StringBuilder();

        public IInsertWriter AppendColumns(ITableIdentifier table, List<ColumnDefinition> columns)
        {
            _sb.Append("INSERT INTO [")
               .Append(table.Database)
               .Append("].[")
               .Append(table.Schema)
               .Append("].[")
               .Append(table.Table)
               .Append("](");

            //Nom des colonnes
            for (var i = 0; i < columns.Count(); i++)
            {
                if (! columns[i].IsAutoIncrement)
                    _sb.Append('[').Append(columns[i].Name).Append(']').Append(",");
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
            _sb.Append("'").Append(value).Append("',");
            return this;
        }

        public IInsertWriter AppendVariable(string varName)
        {
            _sb.Append("@").Append(varName).Append(",");
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
