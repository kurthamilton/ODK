using System;
using System.Data;
using FluentAssertions;
using NUnit.Framework;

namespace ODK.Data.Sql.Tests
{
    public static class SqlDbTypesTests
    {
        [Test]
        public static void GetSqlDbType_NullableDateTime()
        {
            SqlDbType type = SqlDbTypes.GetSqlDbType<DateTime?>();

            type.Should().Be(SqlDbType.DateTime);
        }
    }
}
