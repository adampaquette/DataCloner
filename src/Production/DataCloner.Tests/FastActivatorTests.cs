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
    public class FastActivatorTests
    {
        [Fact]
        public void FastActivatorNoParamCtor()
        {
            Type t = Type.GetType("System.Text.StringBuilder");
            object result = null;

            for (int i = 0; i < 2; i++)
                result = FastActivator.GetConstructor(t);

            Assert.NotNull(result);
        }

        [Fact]
        public void FastActivatorOneParamCtor()
        {
            Type t = Type.GetType("System.String");
            object result = null;

            for (int i = 0; i < 2; i++)
                result = FastActivator<char[]>.GetConstructor(t, new Type[] { typeof(char[]) });

            Assert.NotNull(result);
        }

        [Fact]
        public void FastActivatorTwoParamCtor()
        {
            Type t = Type.GetType("System.String");
            object result = null;

            for (int i = 0; i < 2; i++)
                result = FastActivator<char, int>.GetConstructor(t, new Type[] { typeof(char), typeof(int) });

            Assert.NotNull(result);
        }

        [Fact]
        public void FastActivatorThreeParamCtor()
        {
            Type t = Type.GetType("System.String");
            object result = null;

            for (int i = 0; i < 2; i++)
                result = FastActivator<char[], int, int>.GetConstructor(t, new Type[] { typeof(char[]), typeof(int), typeof(int) });

            Assert.NotNull(result);
        }

        [Fact]
        public void FastActivatorOneParamCtorButFoundMore()
        {
            Type t = Type.GetType("System.String");
            object result = null;

            Assert.Throws(typeof(ArgumentException), () =>
            {
                result = FastActivator<char[]>.GetConstructor(t, new Type[] { typeof(char[]), typeof(int) });
            });
        }

        [Fact]
        public void FastActivatorTwoParamCtorButFoundLess()
        {
            Type t = Type.GetType("System.String");
            object result = null;

            Assert.Throws(typeof(ArgumentException), () => 
            {
                result = FastActivator<char, int>.GetConstructor(t, new Type[] { typeof(char) });
            });
        }

        [Fact]
        public void FastActivatorThreeParamCtorButFoundLess()
        {
            Type t = Type.GetType("System.String");

            Assert.Throws(typeof(ArgumentException), () =>
            {
                FastActivator<char, int, int>.GetConstructor(t, new Type[] { typeof(char*), typeof(int)});
            });
        }
    }
}
