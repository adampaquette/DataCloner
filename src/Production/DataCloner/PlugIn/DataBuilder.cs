using System;
using System.Collections.Generic;
using System.Data;
using DataCloner.DataAccess;
using DataCloner.DataClasse.Cache;
using DataCloner.Framework;

namespace DataCloner.PlugIn
{
    internal static class DataBuilder
    {
        private static readonly Dictionary<string, IDataBuilder> CachedBuilders;

        static DataBuilder()
        {
            CachedBuilders = new Dictionary<string, IDataBuilder>
            {
                {"AutoIncrementDataBuilder", new AutoIncrementDataBuilder()},
                {"StringDataBuilder", new StringDataBuilder()}
            };
        }

        public static void BuildDataFromTable(IQueryHelper queryHelper, string database, ITableSchema table, object[] dataRow)
        {
            if (table.ColumnsDefinition.Length != dataRow.Length)
                throw new ArgumentException(
                    String.Format("The number of columns defined in the cached table {0} '{1}' is different from the current row '{2}'.",
                    table.Name, table.ColumnsDefinition.Length, dataRow.Length));

            //TODO : REGROUPER LES PK ENSEMBLE CAR SI LA LIGNE per exemple 1-1 existe, il ne faut pas générer 2-2 mais 1-2.
            for (var i = 0; i < table.ColumnsDefinition.Length; i++)
            {
                var mustGenerate = false;
                var col = table.ColumnsDefinition[i];
                IDataBuilder builder = null;

                if (!string.IsNullOrWhiteSpace(col.BuilderName))
                {
                    mustGenerate = true;

                    if (!CachedBuilders.ContainsKey(col.BuilderName))
                    {
                        var t = Type.GetType(col.BuilderName);
                        builder = FastActivator.CreateInstance(t) as IDataBuilder;
                        CachedBuilders.Add(col.BuilderName, builder);
                    }
                    else
                        builder = CachedBuilders[col.BuilderName];
                }
                else if ((col.IsPrimary && !col.IsAutoIncrement && !col.IsForeignKey) || col.IsUniqueKey)
                {
                    mustGenerate = true;
                    switch (col.Type)
                    {
                        case DbType.Byte:
                        case DbType.Decimal:
                        case DbType.Double:
                        case DbType.SByte:
                        case DbType.Single:
                        case DbType.Int16:
                        case DbType.Int32:
                        case DbType.Int64:
                        case DbType.UInt16:
                        case DbType.UInt32:
                        case DbType.UInt64:
                            builder = CachedBuilders["AutoIncrementDataBuilder"]; 
                            break;
                        case DbType.AnsiString:
                        case DbType.AnsiStringFixedLength:
                        case DbType.Guid:
                        case DbType.String:
                        case DbType.StringFixedLength:
                            builder = CachedBuilders["StringDataBuilder"];
                            break;
                        default:
                            throw new NotSupportedException(
                                String.Format("The generation of the key failed. Please specify a databuilder in the configuration for {0}.{1}.{2}",
                                database, table, col.Name));
                    }
                }

                //Generate data
                if (mustGenerate)
                {
                    if (builder == null)
                        throw new NullReferenceException(
                            String.Format("Builder '{0}' for column '{1}' is not found. Watch configuration file.", col.BuilderName, col.Name));
                    dataRow[i] = builder.BuildData(queryHelper.Connection, queryHelper.Engine, database, table, col);
                }
            }
        }
    }
}
