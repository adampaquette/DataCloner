using DataCloner.DataClasse.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DataCloner.Framework;
using System.Data;
using SqlFu;

namespace DataCloner.PlugIn
{
    internal static class DataBuilder
    {
        public static void BuildDataFromTable(IDbConnection conn, ITableDef table, ref object[] dataRow)
        {
            //TODO:Cache instance of each builder
            if(table.SchemaColumns.Length != dataRow.Length)
                throw new ArgumentException(
                    String.Format("The number of columns defined in the cached table {0} '{1}' is different from the current row '{2}'.", 
                    table.Name, table.SchemaColumns.Length, dataRow.Length));

            for (int i =0; i<table.SchemaColumns.Length; i++)
            {
                bool mustGenerate = false;
                var col = table.SchemaColumns[i];
                IDataBuilder builder = null;

                if (!string.IsNullOrWhiteSpace(col.BuilderName))
                {
                    mustGenerate = true;
                    Type t = Type.GetType(col.BuilderName);
                    builder = FastActivator.CreateInstance(t) as IDataBuilder;
                }
                else if (col.IsPrimary && !col.IsAutoIncrement)
                {
                    mustGenerate = true;
                    switch (col.Type)
                    {
                        case DbType.Int16:
                        case DbType.Int32:
                        case DbType.Int64:
                            builder = new AutoIncrementDataBuilder();
                            break;
                        case DbType.String:
                            builder = new StringDataBuilder();
                            break;
                    }
                }

                //Generate data
                if (mustGenerate)
                {
                    if (builder == null)
                        throw new NullReferenceException(
                            String.Format("Builder '{0}' for column '{1}' is not found. Watch configuration file.", col.BuilderName, col.Name));
                    else
                        dataRow[i] = builder.BuildData(conn, table, col);
                }
            }
        }
    }
}
