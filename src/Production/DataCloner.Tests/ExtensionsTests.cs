using System;
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
    public class ExtensionsTests
    {
        [Fact]
        public void XmlSerialization()
        {
            var lst = new List<string>{"xml", "Serialization", "test"};
            var str = lst.SerializeXml();
            var strOutput = str.DeserializeXml<List<string>>().SerializeXml();

            Assert.Equal(str, strOutput);
        }

        [Fact]
        public void ArrayRemoveAt()
        {
            var t = new int[] { 1, 2, 3, 4 };
            var m = t.RemoveAt(0);
            Assert.True(m.SequenceEqual(new int[] { 2, 3, 4 }));

            m = m.RemoveAt(2);
            Assert.True(m.SequenceEqual(new int[] { 2, 3 }));
        }

        [Fact]
        public void ArrayRemove()
        {
            var t = new int[] { 1, 2, 3, 4 };
            var n = t.Remove(4);
            Assert.True(n.SequenceEqual(new int[] { 1, 2, 3 }));
        }

        [Fact]
        public void ArrayAdd()
        {
            var t = new int[] { 1, 2, 3, 4 };
            var l = t.Add(5).Add(1);
            Assert.True(l.SequenceEqual(new int[] { 1, 2, 3, 4, 5, 1 }));
        }
    }
}
