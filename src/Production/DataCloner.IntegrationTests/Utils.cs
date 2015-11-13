using DataCloner.Configuration;
using DataCloner.Data;
using System.Collections.Generic;

namespace DataCloner.IntegrationTests
{
    public class Utils
    {
        public static Settings MakeDefaultSettings(SqlConnection conn)
        {
            return new Settings
            {
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
                    }
                }
            };
        }
    }
}
