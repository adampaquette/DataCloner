using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using DataCloner;
using DataCloner.DataAccess;
using DataCloner.DataClasse;
using DataCloner.DataClasse.Cache;
using DataCloner.DataClasse.Configuration;
using DataCloner.Framework;
using DataCloner.Enum;

using Class;
using Xunit;

namespace DataCloner.Tests
{
    public class KeyRelationshipTests
    {
        private KeyRelationship _keys;

        public KeyRelationshipTests()
        {
            _keys = new KeyRelationship();
            _keys.SetKey(1, "db", "dbo", "table1", new object[] { 1, 1 }, new object[] { 1, 2 });
            _keys.SetKey(1, "db", "dbo", "table1", new object[] { 1, 2 }, new object[] { 1 });
        }

        [Fact()]
        public void GetKeyValueTests()
        {
            object[] key = _keys.GetKey(1, "db", "dbo", "table1", new object[] { 1, 1 });
            object[] key2 = _keys.GetKey(1, "db", "dbo", "table1", new object[] { 1, 2 });

            Assert.True(StructuralComparisons.StructuralEqualityComparer.Equals(new object[] { 1, 2 }, key));
            Assert.True(StructuralComparisons.StructuralEqualityComparer.Equals(new object[] { 1 }, key2));
        }      
    }
}
