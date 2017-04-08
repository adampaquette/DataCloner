using DataCloner.Core.Configuration;
using DataCloner.Core.Plan;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataCloner.Core.Debug
{
    public static class Utils
    {
        public static string TestDatabase(Connection conn)
        {
            if (conn.Id == "UNI_Chinook")
                return "Chinook";
            else if (conn.Id == "UNI_ChinookAI")
                return "ChinookAI";
            else
                throw new NotSupportedException();
        }

        public static string TestSchema(Connection conn)
        {
            if (conn.ProviderName == "MySql.Data.MySqlClient")
                return "";
            else if (conn.ProviderName == "Npgsql")
                return "public";
            else
                return "dbo";
        }

        public static CloningContext MakeDefaultContext()
        {
            return new CloningContext
            {
                UseInMemoryCacheOnly = true,
                Behaviour = "Default",
                SourceEnvironment = "UNI",
                DestinationEnvironment = "UNI"
            };
        }

        public static Project MakeDefaultProject(Connection conn)
        {
            return new Project
            {
                Name = "Chinook",
                ConnectionStrings = new List<Connection>
                {
                    new Connection
                    {
                        Id = conn.Id,
                        ConnectionString = conn.ConnectionString,
                        ProviderName = conn.ProviderName
                    }
                },
                EnvironmentComposition = new List<SchemaVar>
                {
                    new SchemaVar {Id = "chinook", Server = conn.Id, Database = TestDatabase(conn), Schema = TestSchema(conn)}
                },
                ExtractionTemplates = new List<DbSettings>
                {
                    new DbSettings
                    {
                        ForSchemaId = "chinook",
                        Id = "Default",
                        Tables = new List<Table>()
                    }
                },
                ExtractionBehaviors = new List<Behavior>
                {
                    new Behavior
                    {
                        Id = "Default",
                        DbSettings = new List<DbSettings>
                        {
                            new DbSettings{Id = "1", BasedOn = "Default"}
                        }
                    }
                },
                Environments = new List<Configuration.Environment>
                {
                    new Configuration.Environment
                    {
                        Name = "UNI",
                        Schemas = new List<SchemaVar>
                        {
                            new SchemaVar
                            {
                                Id = "chinook",
                                Server = conn.Id,
                                Database = TestDatabase(conn),
                                Schema = TestSchema(conn)
                            }
                        }
                    }
                }
            };
        }

        public static bool ScrambledEquals<T>(IEnumerable<T> list1, IEnumerable<T> list2, IEqualityComparer<T> comparer)
        {
            var cnt = new Dictionary<T, int>(comparer);
            foreach (T s in list1)
            {
                if (cnt.ContainsKey(s))
                    cnt[s]++;
                else
                    cnt.Add(s, 1);
            }
            foreach (T s in list2)
            {
                if (cnt.ContainsKey(s))
                    cnt[s]--;
                else
                    return false;
            }
            return cnt.Values.All(c => c == 0);
        }

        public static bool IsNumericType(this object o)
        {
            switch (Type.GetTypeCode(o.GetType()))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }
    }
}
