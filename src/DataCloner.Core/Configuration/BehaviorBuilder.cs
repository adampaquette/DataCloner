using System;
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
        public static Behavior BuildBehavior(this ConfigurationProject project, Int16 behaviorId)
        {
            if (project == null)
                throw new ArgumentNullException(nameof(project));

            //Find the behavior
            var behavior = project.Behaviors.FirstOrDefault(b => b.Id == behaviorId);
            if (behavior == null)
                throw new KeyNotFoundException($"The behavior Id {behaviorId} was not found in the configuration file.");

            var compiledBehavior = new Behavior();

            //DbSettings only have a single inheritance hierarchy so we have to destack from the top.
            foreach (var setting in behavior.DbSettings)
            {
                var stack = new Stack<DbSettings>();
                stack.Push(setting);

                //Stack
                while (stack.Peek().BasedOn > 0)
                {
                    var current = stack.Peek();

                    var parent = project.Templates.FirstOrDefault(t => t.Id == current.BasedOn);
                    if (parent == null)
                        throw new KeyNotFoundException($"The DbSetting with Id {current.BasedOn} was not found in the configuration file." +
                                                       $"Remove the attribute BasedOn=\"{current.BasedOn}\".");

                    stack.Push(parent);
                }

                //Destack and merge
                var compiledDbSettings = new DbSettings();
                while (stack.Count > 0)
                    MergeDbSettings(stack.Pop(), compiledDbSettings);

                //Si le résultat du merge est sur la même variable qu'un autre setting compilée alors on merge par dessus.
                var dup = compiledBehavior.DbSettings.FirstOrDefault(b => b.Var == compiledDbSettings.Var);
                if (dup != null)
                    MergeDbSettings(compiledDbSettings, dup);
            }

            return compiledBehavior;
        }

        /// <summary>
        /// Merge the configuration of two elements in the same hierarchy.
        /// </summary>
        /// <param name="source">Child to append over a parent.</param>
        /// <param name="target">Parent to be overrided.</param>
        public static void MergeDbSettings(DbSettings source, DbSettings target)
        {
            if (!String.IsNullOrWhiteSpace(source.Description))
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
        public static void MergeTable(Table source, Table target)
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
                                                                                           d.Destination == sourceTable.Destination);
                if (targetTable == null)
                    target.DerativeTableGlobal.DerivativeTables.Add(targetTable);
                else
                    MergeDerivativeSubTable(sourceTable, targetTable);
            }

            //ForeignKeys
            foreach (var sourceColumn in source.ForeignKeys.ForeignKeyRemove.Columns)
            {
                //TODO : Supprimer les FKAdd de la target qui ont la sourceColumn en commun.

                var targetForeignKey = target.ForeignKeys.ForeignKeyRemove.Columns.FirstOrDefault(c => c == sourceColumn);

                if (targetForeignKey == null)
                    target.ForeignKeys.ForeignKeyRemove.Columns.Add(targetForeignKey);
            }

            foreach (var sourceForeignKey in source.ForeignKeys.ForeignKeyAdd)
            {
                //TODO : Supprimer les FKRemove de la target qui ont la sourceColumn en commun la sourceForeignKey.

                var targetForeignKey = target.ForeignKeys.ForeignKeyAdd.FirstOrDefault(fk => fk == sourceForeignKey);

                if (targetForeignKey == null)
                    target.ForeignKeys.ForeignKeyAdd.Add(targetForeignKey);
            }
        }

        /// <summary>
        /// Merge the configuration of two elements in the same hierarchy.
        /// </summary>
        /// <param name="source">Child to append over a parent.</param>
        /// <param name="target">Parent to be overrided.</param>
        public static void MergeDataBuilder(DataBuilder source, DataBuilder target)
        {
            target.BuilderName = source.BuilderName;
        }

        /// <summary>
        /// Merge the configuration of two elements in the same hierarchy.
        /// </summary>
        /// <param name="source">Child to append over a parent.</param>
        /// <param name="target">Parent to be overrided.</param>
        public static void MergeDerivativeSubTable(DerivativeTable source, DerivativeTable target)
        {
            if (source.Access != DerivativeTableAccess.NotSet)
                target.Access = source.Access;
            if (source.Cascade != NullableBool.NotSet)
                target.Cascade = source.Cascade;
        }
    }
}