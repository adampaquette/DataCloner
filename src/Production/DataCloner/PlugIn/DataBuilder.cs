﻿using System;
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

        public static object BuildDataColumn(IQueryHelper queryHelper, Int16 serverId, string database, string schema, ITableSchema table, IColumnDefinition col)
        {
            IDataBuilder builder = null;
            var mustGenerate = false;

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
            else if (((col.IsPrimary && !col.IsAutoIncrement) || col.IsUniqueKey) && !col.IsForeignKey)
            {
                mustGenerate = true;
                switch (col.DbType)
                {
                    case DbType.Date:
                    case DbType.DateTime:
                    case DbType.DateTime2:
                        return DateTime.Now;
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
                            $"The generation of the key failed. Please specify a databuilder in the configuration for {database}.{table}.{col.Name}");
                }
            }

            //Generate data
            if (mustGenerate)
            {
                if (builder == null)
                    throw new NullReferenceException(
                        $"Builder '{col.BuilderName}' for column '{col.Name}' is not found. Watch configuration file.");
                return builder.BuildData(queryHelper.Connection, queryHelper.Engine, serverId, database, schema, table, col);
            }
            return null;
        }

        public static void BuildDataFromTable(IQueryHelper queryHelper, Int16 serverId, string database, string schema, ITableSchema table, object[] dataRow)
        {
            if (table.ColumnsDefinition.Length != dataRow.Length)
                throw new ArgumentException(
                    $"The number of columns defined in the cached table {table.Name} '{table.ColumnsDefinition.Length}' is different from the current row '{dataRow.Length}'.");

            for (var i = 0; i < table.ColumnsDefinition.Length; i++)
            {
                var col = table.ColumnsDefinition[i];
                dataRow[i] = BuildDataColumn(queryHelper, serverId, database, schema, table, col);
            }
        }

        public static void ClearBuildersCache()
        {
            foreach (var builder in CachedBuilders)
                builder.Value.ClearCache();
        }
    }
}
