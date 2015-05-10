using DataCloner.DataClasse.Cache;
using System;
using System.Linq;
using System.Text;

namespace DataCloner.Generator
{
    internal class InsertWriterMsSql : IInsertWriter
    {
        private StringBuilder _sb = new StringBuilder();

        public IInsertWriter AppendColumns(string database, TableSchema schema)
        {
            _sb.Append("INSERT INTO \"")
               .Append(database)
               .Append("\".\"")
               .Append(schema.Name)
               .Append("\"(");

            //Nom des colonnes
            for (var i = 0; i < schema.ColumnsDefinition.Count(); i++)
            {
                var col = schema.ColumnsDefinition[i];
                if (!col.IsAutoIncrement)
                    _sb.Append('"').Append(col.Name).Append('"').Append(",");
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
