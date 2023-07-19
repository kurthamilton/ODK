using System;
using NUnit.Framework;
using ODK.Data.Sql.Reflection;

namespace ODK.Data.Sql.Tests.Reflection
{
    public static class ExpressionExtensionsTests
    {
        [Test]
        public static void GetMemberName_Performance()
        {
            DateTime start = DateTime.Now;

            const int iterations = 1000;
            for (int i = 0; i < iterations; i++)
            {
                string name = ExpressionExtensions.GetMemberName<TestEntity, string>(x => x.String);
            }

            DateTime end = DateTime.Now;

            double average = (end - start).TotalMilliseconds / iterations;
        }
    }
}
