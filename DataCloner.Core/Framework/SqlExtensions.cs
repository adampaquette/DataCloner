using System;
using System.Data;

namespace DataCloner.Core.Framework
{
    public static class SqlExtensions
    {
        /// <summary>
        /// Build a SQL text query from a DbCommand.
        /// </summary>
        /// <param name="dbCommand">The query</param>
        /// <returns>SQL query</returns>
        public static string GetGeneratedQuery(this IDbCommand dbCommand)
        {
            var query = dbCommand.CommandText;
            foreach (var parameter in dbCommand.Parameters)
            {
                var param = parameter as IDataParameter;
                if (param == null)
                    throw new Exception();
                string newValue;

                if (param.Direction == ParameterDirection.Output)
                    newValue = param.ParameterName + "/*" + param.Value.ToString().EscapeSql() + "*/";
                else
                    newValue = param.Value.ToString().EscapeSql();

                query = query.Replace(param.ParameterName + " ", newValue + " ");
                query = query.Replace(param.ParameterName + ",", newValue + ",");
                query = query.Replace(param.ParameterName + ")", newValue + ")");
            }
            return query;
        }

        internal static string FormatSqlParam(this string value)
        {
            return value.Replace(" ", string.Empty);
        }

        internal static string EscapeSql(this string value)
        {
            return value.Replace("'", "''");
        }
    }
}
