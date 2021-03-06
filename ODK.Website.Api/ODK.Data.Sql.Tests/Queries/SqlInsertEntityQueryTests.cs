﻿using System.Linq;
using NUnit.Framework;
using ODK.Data.Sql.Mapping;
using ODK.Data.Sql.Queries;
using ODK.Data.Sql.Tests.Mapping;

namespace ODK.Data.Sql.Tests.Queries
{
    public static class SqlInsertEntityQueryTests
    {
        [Test]
        public static void GetParameterValues_ReturnsEntityValues()
        {
            TestEntityMap map = new TestEntityMap("Table");
            map.AddProperty(x => x.Int);
            map.AddProperty(x => x.String);

            TestEntity entity = new TestEntity
            {
                Int = 5,
                String = "value"
            };

            SqlContext context = CreateMockContext();
            SqlQuery<TestEntity> query = new SqlInsertEntityQuery<TestEntity>(context, entity);

            (SqlColumn Column, string ParameterName, object Value)[] parameterValues = query.GetParameterValues(context).ToArray();

            CollectionAssert.AreEqual(new[] { "Int", "String" }, parameterValues.Select(x => x.Column.ColumnName));
            CollectionAssert.AreEqual(new object[] { 5, "value" }, parameterValues.Select(x => x.Value));
        }

        [Test]
        public static void ToSql_ReturnsParameterisedSql()
        {
            TestEntityMap map = new TestEntityMap("Table");
            map.AddProperty(x => x.Int);
            map.AddProperty(x => x.String);

            SqlContext context = CreateMockContext(map);

            SqlQuery<TestEntity> query = new SqlInsertEntityQuery<TestEntity>(context, new TestEntity());

            string sql = query.ToSql(context);

            Assert.AreEqual("INSERT INTO Table ([Int],[String]) VALUES (@Int,@String)", sql);
        }

        [Test]
        public static void ToSql_MapsColumnName()
        {
            TestEntityMap map = new TestEntityMap("Table");
            map.AddProperty(x => x.Int);
            map.AddProperty(x => x.String).HasColumnName("Other");

            SqlContext context = CreateMockContext(map);

            SqlQuery<TestEntity> query = new SqlInsertEntityQuery<TestEntity>(context, new TestEntity());

            string sql = query.ToSql(context);

            Assert.AreEqual("INSERT INTO Table ([Int],[Other]) VALUES (@Int,@Other)", sql);
        }

        [Test]
        public static void ToSql_IgnoresIdentityColumn()
        {
            TestEntityMap map = new TestEntityMap("Table");
            map.AddProperty(x => x.Int).IsIdentity();
            map.AddProperty(x => x.String);

            SqlContext context = CreateMockContext(map);

            SqlQuery<TestEntity> query = new SqlInsertEntityQuery<TestEntity>(context, new TestEntity());

            string sql = query.ToSql(context);

            Assert.AreEqual("INSERT INTO Table ([String]) VALUES (@String)", sql);
        }

        [Test]
        public static void ToSql_WithOutputIdentity_AddsIdentityColumnAsOutput()
        {
            TestEntityMap map = new TestEntityMap("Table");
            map.AddProperty(x => x.Int).IsIdentity();
            map.AddProperty(x => x.String);

            SqlContext context = CreateMockContext(map);

            SqlInsertEntityQuery<TestEntity> query = new SqlInsertEntityQuery<TestEntity>(context, new TestEntity());
            query.OutputIdentity();

            string sql = query.ToSql(context);

            Assert.AreEqual("INSERT INTO Table ([String]) OUTPUT inserted.Int VALUES (@String)", sql);
        }

        private static SqlContext CreateMockContext(SqlMap<TestEntity> map = null)
        {
            MockContext context = new MockContext();
            context.AddMockMap(map ?? CreateMap());
            return context;
        }

        private static SqlMap<TestEntity> CreateMap()
        {
            TestEntityMap map = new TestEntityMap();
            map.AddProperty(x => x.Int);
            map.AddProperty(x => x.String);
            return map;
        }
    }
}
