using DataCloner.Core.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DataCloner.Core.Metadata.Context
{
    internal static class MetadataLoader
    {
        /// <summary>
        /// Load the result of an Sql query into the metadatas object.
        /// </summary>
        /// <param name="reader">Result of an Sql query defined in <see cref="MetadataProvider.SqlGetForeignKeys"/></param>
        /// <param name="metadata">Metadatas container</param>
        /// <param name="serverId">ServerId loaded from</param>
        /// <param name="database">Database loaded from</param>
        internal static void LoadForeignKeys(IDataReader reader, Metadatas metadata, string serverId, string database)
        {
            var lstForeignKeys = new List<ForeignKey>();
            var lstForeignKeyColumns = new List<ForeignKeyColumn>();

            if (!reader.Read())
                return;

            //Init first row
            var currentSchema = reader.GetString(0);
            var previousTable = metadata[serverId][database][currentSchema].First(t => t.Name.Equals(reader.GetString(1), StringComparison.OrdinalIgnoreCase));
            var previousConstraintName = reader.GetString(2);
            var previousConstraint = new ForeignKey
            {
                ServerIdTo = serverId,
                DatabaseTo = database,
                SchemaTo = currentSchema,
                TableTo = reader.GetString(5)
            };

            //Pour chaque ligne
            do
            {
                currentSchema = reader.GetString(0);
                var currentTable = reader.GetString(1);
                var currentConstraint = reader.GetString(2);

                //Si on change de constraint
                if (currentTable != previousTable.Name || currentConstraint != previousConstraintName)
                {
                    previousConstraint.Columns = lstForeignKeyColumns;
                    lstForeignKeys.Add(previousConstraint);

                    lstForeignKeyColumns = new List<ForeignKeyColumn>();
                    previousConstraint = new ForeignKey
                    {
                        ServerIdTo = serverId,
                        DatabaseTo = database,
                        SchemaTo = currentSchema,
                        TableTo = reader.GetString(5)
                    };
                    previousConstraintName = currentConstraint;
                }

                //Si on change de table
                if (currentTable != previousTable.Name)
                {
                    previousTable.ForeignKeys = lstForeignKeys;

                    //Change de table
                    previousTable = metadata[serverId][database][currentSchema].First(t => t.Name.Equals(reader.GetString(1), StringComparison.OrdinalIgnoreCase));
                    lstForeignKeys = new List<ForeignKey>();
                }

                //Ajoute la colonne
                var colName = reader.GetString(3);
                lstForeignKeyColumns.Add(new ForeignKeyColumn
                {
                    NameFrom = colName,
                    NameTo = reader.GetString(6)
                });

                //Affecte l'indicateur dans le schema
                var col = previousTable.ColumnsDefinition.FirstOrDefault(c => c.Name.Equals(colName, StringComparison.OrdinalIgnoreCase));
                if (col == null)
                    throw new Exception($"The column {colName} has not been found in the metadata for the table {previousTable.Name}.");
                col.IsForeignKey = true;
            } while (reader.Read());

            //Ajoute la dernière table / schema
            if (lstForeignKeyColumns.Count > 0)
            {
                previousConstraint.Columns = lstForeignKeyColumns;
                lstForeignKeys.Add(previousConstraint);
                previousTable.ForeignKeys = lstForeignKeys;
            }
        }

        /// <summary>
        /// Load the result of an Sql query into the metadatas object.
        /// </summary>
        /// <param name="reader">Result of an Sql query defined in <see cref="MetadataProvider.SqlGetUniqueKeys"/></param>
        /// <param name="metadata">Metadatas container</param>
        /// <param name="serverId">ServerId loaded from</param>
        /// <param name="database">Database loaded from</param>
        internal static void LoadUniqueKeys(IDataReader reader, Metadatas metadata, string serverId, string database)
        {
            var lstUniqueKeys = new List<UniqueKey>();
            var lstUniqueKeyColumns = new List<string>();

            if (!reader.Read())
                return;

            //Init first row
            var currentSchema = reader.GetString(0);
            var previousTable = metadata[serverId][database][currentSchema].First(t => t.Name.Equals(reader.GetString(1), StringComparison.OrdinalIgnoreCase));
            var previousConstraintName = reader.GetString(2);
            var previousConstraint = new UniqueKey();

            //Pour chaque ligne
            do
            {
                currentSchema = reader.GetString(0);
                var currentTable = reader.GetString(1);
                var currentConstraint = reader.GetString(2);

                //Si on change de constraint
                if (currentTable != previousTable.Name || currentConstraint != previousConstraintName)
                {
                    previousConstraint.Columns = lstUniqueKeyColumns;
                    lstUniqueKeys.Add(previousConstraint);

                    lstUniqueKeyColumns = new List<string>();
                    previousConstraint = new UniqueKey();
                    previousConstraintName = currentConstraint;
                }

                //Si on change de table
                if (currentTable != previousTable.Name)
                {
                    previousTable.UniqueKeys = lstUniqueKeys;

                    //Change de table
                    previousTable = metadata[serverId][database][currentSchema].First(t => t.Name.Equals(reader.GetString(1), StringComparison.OrdinalIgnoreCase));
                    lstUniqueKeys = new List<UniqueKey>();
                }

                //Ajoute la colonne
                var colName = reader.GetString(3);
                lstUniqueKeyColumns.Add(colName);

                //Affecte l'indicateur dans le schema
                var col = previousTable.ColumnsDefinition.FirstOrDefault(c => c.Name.Equals(colName, StringComparison.OrdinalIgnoreCase));
                if (col == null)
                    throw new Exception($"The column {colName} has not been found in the metadata for the table {previousTable.Name}.");
                col.IsUniqueKey = true;
            } while (reader.Read());

            //Ajoute la dernière table / schema
            if (lstUniqueKeyColumns.Count > 0)
            {
                previousConstraint.Columns = lstUniqueKeyColumns;
                lstUniqueKeys.Add(previousConstraint);
                previousTable.UniqueKeys = lstUniqueKeys;
            }
        }

        /// <summary>
        /// Load the result of an Sql query into the metadatas object.
        /// </summary>
        /// <param name="reader">Result of an Sql query defined in <see cref="MetadataProvider.SqlGetColumns"/></param>
        /// <param name="metadata">Metadatas container</param>
        /// <param name="serverId">ServerId loaded from</param>
        /// <param name="database">Database loaded from</param>
        /// <param name="typeConverter">Classe performing type conversion between database and .NET.</param>
		internal static void LoadColumns(IDataReader reader, Metadatas metadata, string serverId, string database, ISqlTypeConverter typeConverter)
        {
            var schemaMetadata = new SchemaMetadata();
            var lstSchemaColumn = new List<ColumnDefinition>();
            string currentSchema;

            if (!reader.Read())
                return;

            //Init first row
            var previousSchema = reader.GetString(0);
            var previousTable = new TableMetadata(reader.GetString(1));

            //Pour chaque ligne
            do
            {
                currentSchema = reader.GetString(0);
                var currentTable = reader.GetString(1);

                //Si on change de table
                if (currentSchema != previousSchema || currentTable != previousTable.Name)
                {
                    previousTable.ColumnsDefinition = lstSchemaColumn;
                    schemaMetadata.Add(previousTable);

                    lstSchemaColumn = new List<ColumnDefinition>();
                    previousTable = new TableMetadata(currentTable);
                }

                //Si on change de schema
                if (currentSchema != previousSchema)
                {
                    metadata[serverId, database, currentSchema] = schemaMetadata;
                    schemaMetadata = new SchemaMetadata();
                }

                //Ajoute la colonne
                var col = new ColumnDefinition
                {
                    Name = reader.GetString(2),
                    SqlType = new SqlType
                    {
                        DataType = reader.GetString(3),
                        Precision = reader.GetInt32(4),
                        Scale = reader.GetInt32(5),
                        IsUnsigned = reader.GetBoolean(6)
                    },
                    IsPrimary = reader.GetBoolean(7),
                    IsAutoIncrement = reader.GetBoolean(8)
                };
                col.DbType = typeConverter.ConvertFromSql(col.SqlType);

                lstSchemaColumn.Add(col);

            } while (reader.Read());

            //Ajoute la dernière table / schema
            if (lstSchemaColumn.Count > 0)
            {
                previousTable.ColumnsDefinition = lstSchemaColumn;
                schemaMetadata.Add(previousTable);
                metadata[serverId, database, currentSchema] = schemaMetadata;
            }
        }
    }
}
