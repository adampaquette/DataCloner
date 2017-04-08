using System;
using System.Collections.Generic;
using DataCloner.Core;
using DataCloner.Core.Configuration;
using DataCloner.Core.Plan;
using DataCloner.Core.Framework;
using System.Text;

class Program
{
    static double NbRowsFetch;

    /// <summary>
    /// DataCloner Hello World program
    /// </summary>
    static void Main(string[] args)
    {
        //Minimal configuration
        var project = new Project()
        {
            ConnectionStrings = new List<Connection>
            {
                new Connection{ Id = "UNI", ProviderName = "System.Data.SqlClient", ConnectionString =@"Data Source=(localdb)\MSSQLLocalDB;Integrated Security=True;"}
            }
        };

        //Creating an execution plan for reuse later
        var builder = new ExecutionPlanBuilder(project, null);
        builder.StatusChanged += Builder_StatusChanged;

        Console.WriteLine("Retreiving rows to the execution plan");

        NbRowsFetch = 0;
        var startTime = DateTime.Now.Ticks;
        builder.Append(new RowIdentifier
        {
            ServerId = "UNI",
            Database = "Chinook",
            Schema = "dbo",
            Table = "Customer",
            Columns = new ColumnsWithValue { { "CustomerId", 1 } }
        });
        var endTime = DateTime.Now.Ticks;

        //Creating a mew clone of the data inside the database
        var query = builder.Compile();
        query.Commiting += Query_Commiting;
        query.Execute();

        //Results
        var msElapsed = new TimeSpan(endTime - startTime).TotalMilliseconds;
        Console.WriteLine($"Rows fetched : {NbRowsFetch}");
        Console.WriteLine($"Completed in : {msElapsed} ms");
        Console.WriteLine($"Row per second : {NbRowsFetch / (msElapsed / 1000)}");

        Console.ReadKey();
    }

    /// <summary>
    /// Show the activity
    /// </summary>
    private static void Builder_StatusChanged(object sender, StatusChangedEventArgs e)
    {
        //Traveling up the tree
        //Base rows or dependencies will be shown as Status.Cloning
        if (e.Status == Status.Cloning)
        {
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.ForegroundColor = ConsoleColor.Cyan;

            var sb = new StringBuilder(new string(' ', 4 * (e.Level)));
            sb.Append(e.SourceRow.Database)
                .Append(".")
                .Append(e.SourceRow.Schema)
                .Append(".")
                .Append(e.SourceRow.Table)
                .Append(" : (");

            foreach (var col in e.SourceRow.Columns)
                sb.Append(col.Key).Append("=").Append(col.Value).Append(", ");
            sb.Remove(sb.Length - 2, 2);
            sb.Append(")");

            Console.WriteLine(sb.ToString());
            NbRowsFetch++;
        }
        //Traveling down the tree
        //Rows using a table PK as a FK inside their table
        else if (e.Status == Status.FetchingDerivatives)
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(new string(' ', 4 * (e.Level - 1)) + "Retreiving derivatives");
        }

        Console.ResetColor();
    }

    /// <summary>
    /// When the query is ready to be executed
    /// </summary>
    private static void Query_Commiting(object sender, QueryCommitingEventArgs e)
    {
        //Don't commit the Sql query
        e.Cancel = true;

        Console.WriteLine();
        Console.WriteLine("Generated query : ");
        Console.BackgroundColor = ConsoleColor.DarkYellow;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(e.Command.GetGeneratedQuery());
        Console.ResetColor();
    }
}