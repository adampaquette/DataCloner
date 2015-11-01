using System;

namespace DataCloner.Configuration
{
    public class ConfigVariable
    {
        public string Key { get; private set; }
        public Int16 Server { get; private set; }
        public string Database { get; private set; }
        public string Schema { get; private set; }

        public ConfigVariable(string key, Int16 server, string database, string schema)
        {
            Key = key;
            Server = server;
            Database = database;
            Schema = schema;
        }
    }

    public static class ConfigVariableParser
    {
        /// <summary>
        /// Extract variable from a syntaxe like {$KEY{SERVER_VALUE}{DATABASE_VALUE_OPTIONAL}{SCHEMA_VALUE_OPTIONAL}} OR {$DATABASE_SOURCE{1}{MyDb}}.
        /// </summary>
        /// <param name="value">String variable</param>
        /// <returns>String variable parsed into ConfigVariable.</returns>
        public static ConfigVariable ParseConfigVariable(this string value)
        {
            var pos = value.IndexOf("{$");
            if (pos != 0) return null;

            int currentPos = 1;
            int endPosKey = 0;
            Int16 server = 0;
            string key, database, schema;
            key = database = schema = string.Empty;

            for (var i = 0; i < 3; i++)
            {
                var posVar = value.IndexOf('{', ++currentPos);
                if (posVar == -1) break;
                var posVarEnd = value.IndexOf('}', ++posVar);

                if (i == 0)
                {
                    endPosKey = posVar-1;
                    Int16.TryParse(value.Substring(posVar, posVarEnd - posVar), out server);
                }
                else if (i == 1)
                    database = value.Substring(posVar, posVarEnd - posVar).ToLower();
                else if (i == 2)
                    schema = value.Substring(posVar, posVarEnd - posVar).ToLower();

                currentPos = posVarEnd;
            }

            pos = value.IndexOf('}', currentPos);
            if (pos == -1) return null;

            if (endPosKey == 0) endPosKey = pos;
            key = value.Substring(2, endPosKey - 2);

            return new ConfigVariable(key, server, database, schema);
        }

        public static Int16 GetServer(this string value)
        {
            if (value.IsVariable())
                return value.ParseConfigVariable().Server;

            Int16 server;
            Int16.TryParse(value, out server);
            return server;
        }

        public static string GetDatabase(this string value)
        {
            if (value.IsVariable())
                return value.ParseConfigVariable().Database;
            return value.ToLower();
        }

        public static string GetSchema(this string value)
        {
            if (value.IsVariable())
                return value.ParseConfigVariable().Schema;
            return value.ToLower();
        }


        public static bool IsVariable(this string value)
        {
            if (value == null) return false;
            return value.StartsWith("{$") && value.EndsWith("}");
        }
    }
}
