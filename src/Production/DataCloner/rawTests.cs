using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Data;
using System.Security.Cryptography;
using System.Diagnostics;

using DataCloner.DataAccess;
using DataCloner.DataClasse;
using DataCloner.DataClasse.Cache;
using DataCloner.DataClasse.Configuration;
using DataCloner.Framework;
using DataCloner.Enum;
using DataCloner;

using System.Data.SQLite;

using Murmur;

namespace Class
{
    public class main
    {
        static int Main(string[] args)
        {
#if DEBUG
            DataclonerTest1();
#endif
            return 0;
        }
#if DEBUG

        public static void DataclonerTest1()
        {
            var dc = new DataCloner.DataCloner();
            RowIdentifier source = new RowIdentifier();

            //Map serveur source / destination
            dc.ServerMap.Add(new ServerIdentifier { ServerId = 1, Database = "sakila" }, new ServerIdentifier { ServerId = 1, Database = "sakila" });
            dc.ServerMap.Add(new ServerIdentifier { ServerId = 1, Database = "employees" }, new ServerIdentifier { ServerId = 1, Database = "employees" });

            dc.Initialize();


            SQLiteConnection.CreateFile("testDB.sqlite");

            


            /*******************
                 Employees
            *******************/
            //source.ServerId = 1;
            //source.Database = "employees";
            //source.Schema = "";
            //source.Table = "employees";
            //source.Columns.Add("emp_no", 10001);
            //dc.SqlTraveler(source, true, false);


            /*******************
                   SAKILA
            *******************/

            ////Basic test : 1 row
            //source.ServerId = 1;
            //source.Database = "sakila";
            //source.Schema = "";
            //source.Table = "actor";
            //source.Columns.Add("actor_id", 1);
            //dc.SqlTraveler(source, true, false);

            ////Basic test : 1 rows with dependencies
            //source.Columns.Clear();
            //source.ServerId = 1;
            //source.Database = "sakila";
            //source.Schema = "";
            //source.Table = "city";
            //source.Columns.Add("city_id", 9);
            //dc.SqlTraveler(source, true, false);

            //Medium test : 1 rows with lots of dependencies
            source.Columns.Clear();
            source.ServerId = 1;
            source.Database = "sakila";
            source.Schema = "";
            source.Table = "customer";
            source.Columns.Add("active", 0);
            source.Columns.Add("address_id", 20);
            dc.SqlTraveler(source, true, false);
        }
#endif
    }
}