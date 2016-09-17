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

        public static CloningContext MakeDefaultContext()
        {
            return new CloningContext
            {
                UseInMemoryCacheOnly = true,
                BehaviourId = 1,
                From = "UNI",
                To = "UNI"
            };
        }

        public static ConfigurationProject MakeDefaultProject(Connection conn)
        {
            return new ConfigurationProject
            {
                Name = "Chinook",
                ConnectionStrings = new List<Connection>
                {
                    new Connection
                    {
                        Id = conn.Id,
                        Name = conn.Name,
                        ConnectionString = conn.ConnectionString,
                        ProviderName = conn.ProviderName
                    }
                },
                Variables = new List<Variable>
                {
                    new Variable {Name = "chinookFrom", Server = 1, Database = "chinook", Schema = "dbo"}
                },
                Templates = new List<DbSettings>
                {
                    new DbSettings
                    {
                        Var = "chinookFrom",
                        Id = 1,
                        Tables = new List<Table>()
                    }
                },
                Behaviors = new List<Behavior>
                {
                    new Behavior
                    {
                        Name = "Default",
                        Id = 1,
                        DbSettings = new List<DbSettings>
                        {
                            new DbSettings{BasedOn = 1}
                        }
                    }
                },
                Maps = new List<MapFrom>
                {
                    new MapFrom
                    {
                        Name = "UNI",
                        UsableBehaviours = "1",
                        MapTos = new List<MapTo>
                        {
                            new MapTo
                            {
                                Name = "UNI",
                                Variables = new List<Variable>
                                {
                                    new Variable {Name = "chinookTo", Server = 1, Database = "chinook", Schema = "dbo"}
                                }
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
    }
}
