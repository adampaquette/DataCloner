using System;
using DataCloner.DataClasse.Cache;
using System.Data;

namespace DataCloner.PlugIn
{
    internal class StringDataBuilder : IDataBuilder
    {
        public object BuildData(IDbConnection conn, DbEngine engine, ITableSchema table, IColumnDefinition column)
        {
            int size = 10;
            Int32.TryParse(column.Size, out size);

            return CreateString(size);
        }

        internal static string CreateString(int stringLength)
        {
            const string allowedChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789!@$?_-";
            char[] chars = new char[stringLength];
            var rd = new Random();

            for (int i = 0; i < stringLength; i++)
            {
                chars[i] = allowedChars[rd.Next(0, allowedChars.Length)];
            }

            return new string(chars);
        }
    }
}


