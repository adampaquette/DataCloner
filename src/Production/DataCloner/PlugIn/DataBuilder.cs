using DataCloner.DataClasse.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DataCloner.Framework;
using System.Data;
using DataCloner.DataAccess;

namespace DataCloner.PlugIn
{
    internal static class DataBuilder
    {
        public static void BuildDataFromTable(IQueryHelper queryHelper, string database, ITableSchema table, object[] dataRow)
        {
            //TODO:Cache instance of each builder
            if (table.ColumnsDefinition.Length != dataRow.Length)
                throw new ArgumentException(
                    String.Format("The number of columns defined in the cached table {0} '{1}' is different from the current row '{2}'.",
                    table.Name, table.ColumnsDefinition.Length, dataRow.Length));

            //TODO : REGROUPER LES PK ENSEMBLE CAR SI LA LIGNE per exemple 1-1 existe, il ne faut pas générer 2-2 mais 1-2.
            for (int i = 0; i < table.ColumnsDefinition.Length; i++)
            {
                bool mustGenerate = false;
                var col = table.ColumnsDefinition[i];
                IDataBuilder builder = null;

                if (!string.IsNullOrWhiteSpace(col.BuilderName))
                {
                    mustGenerate = true;
                    Type t = Type.GetType(col.BuilderName);
                    builder = FastActivator.CreateInstance(t) as IDataBuilder;
                }
                else if (col.IsPrimary && !col.IsAutoIncrement && !col.IsForeignKey)
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
                        dataRow[i] = builder.BuildData(queryHelper.Connection, queryHelper.Engine, database, table, col);
                }
            }
        }
    }
}
