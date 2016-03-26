using System;
using DataCloner.Core.Framework;
using Xunit;

namespace DataCloner.Core.Tests
{
    public class FastActivatorTests
    {
        [Fact]
        public void FastActivatorNoParamCtor()
        {
            var t = Type.GetType("System.Text.StringBuilder");
            object result = null;

            for (var i = 0; i < 2; i++)
                result = FastActivator.GetConstructor(t);

            Assert.NotNull(result);
        }

        [Fact]
        public void FastActivatorOneParamCtor()
        {
            var t = Type.GetType("System.String");
            object result = null;

            for (var i = 0; i < 2; i++)
                result = FastActivator<char[]>.GetConstructor(t, new[] { typeof(char[]) });

            Assert.NotNull(result);
        }

        [Fact]
        public void FastActivatorTwoParamCtor()
        {
            var t = Type.GetType("System.String");
            object result = null;

            for (var i = 0; i < 2; i++)
                result = FastActivator<char, int>.GetConstructor(t, new[] { typeof(char), typeof(int) });

            Assert.NotNull(result);
        }

        [Fact]
        public void FastActivatorThreeParamCtor()
        {
            var t = Type.GetType("System.String");
            object result = null;

            for (var i = 0; i < 2; i++)
                result = FastActivator<char[], int, int>.GetConstructor(t, new[] { typeof(char[]), typeof(int), typeof(int) });

            Assert.NotNull(result);
        }

        [Fact]
        public void FastActivatorOneParamCtorButFoundMore()
        {
            var t = Type.GetType("System.String");

            Assert.Throws(typeof(ArgumentException), () =>
            {
                FastActivator<char[]>.GetConstructor(t, new[] { typeof(char[]), typeof(int) });
            });
        }

        [Fact]
        public void FastActivatorTwoParamCtorButFoundLess()
        {
            var t = Type.GetType("System.String");

            Assert.Throws(typeof(ArgumentException), () => 
            {
                FastActivator<char, int>.GetConstructor(t, new[] { typeof(char) });
            });
        }

        [Fact]
        public void FastActivatorThreeParamCtorButFoundLess()
        {
            var t = Type.GetType("System.String");

            Assert.Throws(typeof(ArgumentException), () =>
            {
                FastActivator<char, int, int>.GetConstructor(t, new[] { typeof(char*), typeof(int)});
            });
        }
    }
}
