using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace DataCloner.Core.Configuration
{
    [Serializable]
    public static class BehaviourBuilder
    {
        /// <summary>
        /// Build the behavior from the templates.
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
                {
                    var source = stack.Peek();
                    var target = stack.Pop();

                    MergeDbSettings(source, target);
                }

                //Si le résultat du merge est sur la même variable qu'un autre setting compilée alors on merge par dessus.
                var dup = compiledBehavior.DbSettings.FirstOrDefault(b => b.Var == compiledDbSettings.Var);
                if (dup != null)
                    MergeDbSettings(compiledDbSettings, dup);
            }

            return compiledBehavior;
        }

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
            if (source.DerativeTables.GlobalAccess != DerivativeTableAccess.NotSet)
                target.DerativeTables.GlobalAccess = source.DerativeTables.GlobalAccess;

            if (source.DerativeTables.GlobalCascade != NullableBool.NotSet)
                target.DerativeTables.GlobalCascade = source.DerativeTables.GlobalCascade;

            foreach (var sourceTable in source.DerativeTables.DerivativeSubTables)
            {
                var targetTable = target.DerativeTables.DerivativeSubTables.FirstOrDefault(d => d.Name == sourceTable.Name &&
                                                                                           d.Destination == sourceTable.Destination);
                if (targetTable == null)
                    target.DerativeTables.DerivativeSubTables.Add(targetTable);
                else
                    MergeDerivativeSubTable(sourceTable, targetTable);
            }

            //ForeignKeys
            foreach (var sourceForeignKey in source.ForeignKeys.ForeignKeyRemove.Columns)
            {
                //Add 
                var targetForeignKey = target.ForeignKeys.ForeignKeyRemove.Columns.FirstOrDefault(c => c == sourceForeignKey);

                if (targetForeignKey == null)
                    target.ForeignKeys.ForeignKeyRemove.Columns.Add(targetForeignKey);
            }

            foreach (var sourceForeignKey in source.ForeignKeys.ForeignKeyAdd)
            {
                var targetForeignKey = target.ForeignKeys.ForeignKeyAdd.FirstOrDefault(fk => fk == sourceForeignKey);

                if (targetForeignKey == null)
                    target.ForeignKeys.ForeignKeyAdd.Add(targetForeignKey);
            }
        }

        public static void MergeDataBuilder(DataBuilder source, DataBuilder target)
        {
            target.BuilderName = source.BuilderName;
        }

        public static void MergeDerivativeSubTable(DerivativeTable source, DerivativeTable target)
        {
            if (source.Access != DerivativeTableAccess.NotSet)
                target.Access = source.Access;
            if (source.Cascade != NullableBool.NotSet)
                target.Cascade = source.Cascade;
        }
    }
}