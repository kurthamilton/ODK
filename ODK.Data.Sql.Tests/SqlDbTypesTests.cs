using System;
using System.Data;
using NUnit.Framework;

namespace ODK.Data.Sql.Tests
{
    public static class SqlDbTypesTests
    {
        [Test]
        public static void GetSqlDbType_NullableDateTime()
        {
            SqlDbType type = SqlDbTypes.GetSqlDbType<DateTime?>();

            Assert.AreEqual(SqlDbType.DateTime, type);
        }
    }
}
