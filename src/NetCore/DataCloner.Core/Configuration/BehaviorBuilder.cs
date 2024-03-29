﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace DataCloner.Core.Configuration
{
    /// <summary>
    /// Contains the fonctionnalities to flatten a multi-hyrarchical DbSetting into a single layer.
    /// </summary>
    [Serializable]
    public static class BehaviourBuilder
    {
        /// <summary>
        /// Build / flatten a multi-hyrarchical DbSetting into a single layer.
        /// </summary>
        /// <remarks>Still returns abstract data with variables.</remarks>
        public static Behavior BuildBehavior(this Project project, string behaviorId)
        {
            if (project == null)
                throw new ArgumentNullException(nameof(project));

            //Find the behavior
            var behavior = project.ExtractionBehaviors.FirstOrDefault(b => b.Id == behaviorId);
            if (behavior == null)
                throw new KeyNotFoundException($"The extraction behavior id {behaviorId} was not found in the configuration file.");

            var compiledBehavior = new Behavior
            {
                Id = behavior.Id,
                Description = behavior.Description
            };

            //DbSettings only have a single inheritance hierarchy so we have to destack from the top.
            foreach (var setting in behavior.DbSettings)
            {
                var stack = new Stack<DbSettings>();
                stack.Push(setting);

                //Stack
                while (!String.IsNullOrWhiteSpace(stack.Peek().BasedOn))
                {
                    var current = stack.Peek();

                    var parent = project.ExtractionTemplates.FirstOrDefault(t => t.Id == current.BasedOn);
                    if (parent == null)
                        throw new KeyNotFoundException($"The DbSetting with Id {current.BasedOn} was not found in the configuration file." +
                                                       $"Remove the attribute BasedOn=\"{current.BasedOn}\".");
                    stack.Push(parent);
                }

                //Destack and merge
                var dbSettingsOnTop = stack.Peek();
                var compiledDbSettings = new DbSettings
                {
                    ForSchemaId = dbSettingsOnTop.ForSchemaId,
                    Description = dbSettingsOnTop.Description
                };
                while (stack.Count > 0)
                    MergeDbSettings(stack.Pop(), compiledDbSettings);

                //Si le résultat du merge est sur la même variable qu'un autre setting compilée alors on merge par dessus.
                var dup = compiledBehavior.DbSettings.FirstOrDefault(b => b.ForSchemaId == compiledDbSettings.ForSchemaId);
                if (dup != null)
                    MergeDbSettings(compiledDbSettings, dup);
                else
                    compiledBehavior.DbSettings.Add(compiledDbSettings);
            }

            return compiledBehavior;
        }

        /// <summary>
        /// Merge the configuration of two elements in the same hierarchy.
        /// </summary>
        /// <param name="source">Child to append over a parent.</param>
        /// <param name="target">Parent to be overrided.</param>
        private static void MergeDbSettings(DbSettings source, DbSettings target)
        {
            if (!string.IsNullOrWhiteSpace(source.Description))
                target.Description = source.Description;

            foreach (var table in source.Tables)
            {
                var targetTable = target.Tables.FirstOrDefault(t => t.Name == table.Name);
                if (targetTable == null)
                    target.Tables.Add(table);
                else
                    MergeTable(table, targetTable);
            }
        }

        /// <summary>
        /// Merge the configuration of two elements in the same hierarchy.
        /// </summary>
        /// <param name="source">Child to append over a parent.</param>
        /// <param name="target">Parent to be overrided.</param>
        private static void MergeTable(Table source, Table target)
        {
            if (source.IsStatic != NullableBool.NotSet)
                target.IsStatic = source.IsStatic;

            //DataBuilders
            foreach (var sourceDataBuilder in source.DataBuilders)
            {
                var targetDataBuilder = target.DataBuilders.FirstOrDefault(d => d.Name == sourceDataBuilder.Name);
                if (targetDataBuilder == null)
                    target.DataBuilders.Add(sourceDataBuilder);
                else
                    MergeDataBuilder(sourceDataBuilder, targetDataBuilder);
            }

            //DerativeTables
            if (source.DerativeTableGlobal.GlobalAccess != DerivativeTableAccess.NotSet)
                target.DerativeTableGlobal.GlobalAccess = source.DerativeTableGlobal.GlobalAccess;

            if (source.DerativeTableGlobal.GlobalCascade != NullableBool.NotSet)
                target.DerativeTableGlobal.GlobalCascade = source.DerativeTableGlobal.GlobalCascade;

            foreach (var sourceTable in source.DerativeTableGlobal.DerivativeTables)
            {
                var targetTable = target.DerativeTableGlobal.DerivativeTables.FirstOrDefault(d => d.Name == sourceTable.Name &&
                                                                                           d.DestinationSchema == sourceTable.DestinationSchema);
                if (targetTable == null)
                    target.DerativeTableGlobal.DerivativeTables.Add(sourceTable);
                else
                    MergeDerivativeSubTable(sourceTable, targetTable);
            }

            //ForeignKeys
            foreach (var sourceColumn in source.ForeignKeys.ForeignKeyRemove.Columns)
            {
                //Delete ForeignKeyAdd from the target that are ForeignKeyRemove in the source.
                for (var i = target.ForeignKeys.ForeignKeyAdd.Count - 1; i > 0; i--)
                {
                    var targetFkAdd = target.ForeignKeys.ForeignKeyAdd[i];
                    if (targetFkAdd.Columns.Any(c => c.Source == sourceColumn.Name))
                        target.ForeignKeys.ForeignKeyAdd.RemoveAt(i);
                }

                //Add ForeignKeyRemove
                var targetColumn = target.ForeignKeys.ForeignKeyRemove.Columns.FirstOrDefault(c => Equals(c, sourceColumn));

                if (targetColumn == null)
                    target.ForeignKeys.ForeignKeyRemove.Columns.Add(sourceColumn);
            }

            foreach (var sourceForeignKey in source.ForeignKeys.ForeignKeyAdd)
            {
                //Delete ForeignKeyRemove from the target that are ForeignKeyAdd in the source.
                for (var i = target.ForeignKeys.ForeignKeyRemove.Columns.Count - 1; i > 0; i--)
                {
                    var targetFkRemove = target.ForeignKeys.ForeignKeyRemove.Columns[i].Name;
                    if (sourceForeignKey.Columns.Select(c => c.Source).Contains(targetFkRemove))
                        target.ForeignKeys.ForeignKeyRemove.Columns.RemoveAt(i);
                }

                //Add ForeignKeyAdd
                var targetForeignKey = target.ForeignKeys.ForeignKeyAdd.FirstOrDefault(fk => Equals(fk, sourceForeignKey));

                if (targetForeignKey == null)
                    target.ForeignKeys.ForeignKeyAdd.Add(sourceForeignKey);
            }
        }

        /// <summary>
        /// Merge the configuration of two elements in the same hierarchy.
        /// </summary>
        /// <param name="source">Child to append over a parent.</param>
        /// <param name="target">Parent to be overrided.</param>
        private static void MergeDataBuilder(DataBuilder source, DataBuilder target)
        {
            target.BuilderName = source.BuilderName;
        }

        /// <summary>
        /// Merge the configuration of two elements in the same hierarchy.
        /// </summary>
        /// <param name="source">Child to append over a parent.</param>
        /// <param name="target">Parent to be overrided.</param>
        private static void MergeDerivativeSubTable(DerivativeTable source, DerivativeTable target)
        {
            if (source.Access != DerivativeTableAccess.NotSet)
                target.Access = source.Access;
            if (source.Cascade != NullableBool.NotSet)
                target.Cascade = source.Cascade;
        }        
    }
}