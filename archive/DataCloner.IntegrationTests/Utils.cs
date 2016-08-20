using DataCloner.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataCloner.Core.IntegrationTests
{
    public static class Utils
    {
        public static string TestDatabase(Connection conn)
        {
            if (conn.Id == 1)
                return "Chinook";
            else if (conn.Id == 2)
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

        public static Settings MakeDefaultSettings(Connection conn)
        {
            return new Settings
            {
                UseInMemoryCacheOnly = true,
                BehaviourId = 1,
                MapId = 1,
                Project = new ProjectContainer
                {
                    Name = "Chinook",
                    ConnectionStrings = new List<Connection>
                    {
                        new Connection
                        {
                            Id = conn.Id,
                            ProviderName = conn.ProviderName,
                            ConnectionString = conn.ConnectionString
                        }
                    },
                    Maps = new List<Map>
                    {
                        new Map
                        {
                            Id = 1,
                            UsableBehaviours = "1"
                        }
                    },
                    Behaviours = new List<Behaviour>
                    {
                        new Behaviour
                        {
                            Id = 1,
                            Modifiers = new Modifiers
                            {
                                ServerModifiers = new List<ServerModifier>
                                {
                                    new ServerModifier
                                    {
                                        Id = conn.Id.ToString(),
                                        Databases = new List<DatabaseModifier>
                                        {
                                            new DatabaseModifier
                                            {
                                                Name = TestDatabase(conn),
                                                Schemas = new List<SchemaModifier>
                                                {
                                                    new SchemaModifier
                                                    {
                                                        Name = TestSchema(conn),
                                                        Tables = new List<TableModifier>{}
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        public static List<TableModifier> GetDefaultSchema(this Settings settings)
        {
            return settings.Project.Behaviours[0].Modifiers.ServerModifiers[0].Databases[0].Schemas[0].Tables;
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
    }
}
