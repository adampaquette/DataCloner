using System;
using System.Collections.Generic;
using System.Linq;
using DataCloner.Core.Framework;

namespace DataCloner.Core.Internal
{
    /// <summary>
    /// Server / database / schema / table / primarykey source value = primarykey destination value
    /// </summary>
    internal sealed class KeyRelationship 
		: Dictionary<Int16, Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<object[], object[]>>>>>
	{
       
        public object[] GetKey(Int16 server, string database, string schema, string table, object[] keyValuesSource)
        {
            if (ContainsKey(server) &&
                this[server].ContainsKey(database) &&
                this[server][database].ContainsKey(schema) &&
                this[server][database][schema].ContainsKey(table) &&
                this[server][database][schema][table].ContainsKey(keyValuesSource))
                return this[server][database][schema][table][keyValuesSource];
            return null;
        }
		
        public object[] GetKey(RowIdentifier sourceKey)
        {
            var rawKey = new object[sourceKey.Columns.Count];
            for(var i=0; i<sourceKey.Columns.Count; i++)
                rawKey[i] = sourceKey.Columns.ElementAt(i).Value;

            return GetKey(sourceKey.ServerId, sourceKey.Database, sourceKey.Schema, sourceKey.Table, rawKey);
        }

        public void SetKey(Int16 server, string database, string schema, string table, object[] keyValuesSource, object[] keyValuesDestination)
        {
            if (!ContainsKey(server))
                Add(server, new Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<object[], object[]>>>>(StringComparer.OrdinalIgnoreCase));

            if (!this[server].ContainsKey(database))
                this[server].Add(database, new Dictionary<string, Dictionary<string, Dictionary<object[], object[]>>>(StringComparer.OrdinalIgnoreCase));

            if (!this[server][database].ContainsKey(schema))
                this[server][database].Add(schema, new Dictionary<string, Dictionary<object[], object[]>>(StringComparer.OrdinalIgnoreCase));

            if (!this[server][database][schema].ContainsKey(table))
                this[server][database][schema].Add(table, new Dictionary<object[], object[]>(StructuralEqualityComparer<object[]>.Default));

            if (this[server][database][schema][table].ContainsKey(keyValuesSource))
                throw new ArgumentException("Key already exist!");
            this[server][database][schema][table][keyValuesSource] = keyValuesDestination;
        }
    }    
}
