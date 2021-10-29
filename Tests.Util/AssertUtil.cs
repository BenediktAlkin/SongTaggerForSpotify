using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Util
{
    public static class AssertUtil
    {
        public static void SequenceEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual)
        {
            Assert.IsNotNull(actual);
            if (!Enumerable.SequenceEqual(expected, actual))
                throw new AssertionException(
                    $"  Expected: {string.Join(',', expected)}{Environment.NewLine}" +
                    $"  But was:  {string.Join(',', actual)}{Environment.NewLine}");
        }
    }
}
