using DataCloner.Core.Configuration;
using DataCloner.Core.Data;
using System;
using System.Collections.Generic;

namespace DataCloner.Core.IntegrationTests
{
    public static class Utils
    {
        public static string TestSchema(Connection conn)
        {
            if (conn.ProviderName == "MySql.Data.MySqlClient")
                return "";
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
                    Name = "chinook",
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
                                             Id = "1",
                                              Databases = new List<DatabaseModifier>
                                              {
                                                   new DatabaseModifier
                                                   {
                                                        Name = "chinook",
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
    }
}
