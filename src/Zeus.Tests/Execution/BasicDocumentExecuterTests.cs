
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Zeus;
using Zeus.Abstractions;
using Zeus.Execution;
using Zeus.Parser;
using Zeus.Resolvers;
using Xunit;

namespace GraphQL.Tests.Execution
{
    public class BasicDocumentExecuterTests
    {

        [Fact]
        public async Task CallGetStringOnSimpleSchemaQuery()
        {
            // arrange
            string expectedResult = Guid.NewGuid().ToString("N");

            Schema schema = Schema.Create(
                @"
                type Query {
                    getString: String
                }
                ",

                c => c.Add("Query", "getString", () => expectedResult)
            );

            QueryDocumentReader queryDocumentReader = new QueryDocumentReader();
            string query =
                @"
                {
                    getString
                }
                ";

            // act
            DocumentExecuter documentExecuter = new DocumentExecuter();
            QueryResult result = await documentExecuter.ExecuteAsync(
                schema, query, null, null, null, CancellationToken.None);

            // assert
            Assert.Null(result.Errors);
            Assert.NotNull(result.Data);
            Assert.True(result.Data.ContainsKey("getString"));
            Assert.Equal(expectedResult, result.Data["getString"]);
        }

        [Fact]
        public async Task CallGetFooAndResolveObject()
        {
            // arrange
            Schema schema = Schema.Create(
                @"
                type Foo
                {
                    a: String!
                    b: String
                    c: Int
                }

                type Query {
                    getFoo: Foo 
                }
                ",

                c => c.Add("Query", "getFoo", () => "something")
                    .Add("Foo", "a", () => "hello")
                    .Add("Foo", "b", () => "world")
                    .Add("Foo", "c", () => 123)
            );

            QueryDocumentReader queryDocumentReader = new QueryDocumentReader();
            string query =
                @"
                {
                    getFoo
                    {
                        x: a
                        y: b
                        z: c
                    }
                }
                ";

            // act
            DocumentExecuter documentExecuter = new DocumentExecuter();
            QueryResult result = await documentExecuter.ExecuteAsync(
                schema, query, null, null, null, CancellationToken.None);

            // assert
            Assert.Null(result.Errors);
            Assert.NotNull(result.Data);
            Assert.True(result.Data.ContainsKey("getFoo"));
            Assert.IsType<Dictionary<string, object>>(result.Data["getFoo"]);

            Dictionary<string, object> fooObject = (Dictionary<string, object>)result.Data["getFoo"];
            Assert.True(fooObject.ContainsKey("x"));
            Assert.True(fooObject.ContainsKey("y"));
            Assert.True(fooObject.ContainsKey("z"));

            Assert.Equal("hello", fooObject["x"]);
            Assert.Equal("world", fooObject["y"]);
            Assert.Equal(123, fooObject["z"]);
        }

        [Fact]
        public async Task CallGetFooAndResolveWithDynamicObject()
        {
            // arrange
            Schema schema = Schema.Create(
                @"
                type Foo
                {
                    a: String!
                    b: String
                    c: Int
                    d: String
                }

                type Query {
                    getFoo: Foo 
                }
                ",

                c => c.Add("Query", "getFoo", () => new FooMock())
                    .Add("Foo", "d", () => "xyz")
            );

            QueryDocumentReader queryDocumentReader = new QueryDocumentReader();
            string query =
                @"
                {
                    getFoo
                    {
                        x: a
                        y: b
                        z: c
                        d
                    }
                }
                ";

            // act
            DocumentExecuter documentExecuter = new DocumentExecuter();

            Stopwatch sw = Stopwatch.StartNew();
            QueryResult result = await documentExecuter.ExecuteAsync(
                schema, query, null, null, null, CancellationToken.None);

            // assert
            Assert.Null(result.Errors);
            Assert.NotNull(result.Data);
            Assert.True(result.Data.ContainsKey("getFoo"));
            Assert.IsType<Dictionary<string, object>>(result.Data["getFoo"]);

            Dictionary<string, object> fooObject = (Dictionary<string, object>)result.Data["getFoo"];
            Assert.True(fooObject.ContainsKey("x"));
            Assert.True(fooObject.ContainsKey("y"));
            Assert.True(fooObject.ContainsKey("z"));

            Assert.Equal("hello", fooObject["x"]);
            Assert.Equal("world", fooObject["y"]);
            Assert.Equal(123, fooObject["z"]);
            Assert.Equal("xyz", fooObject["d"]);
        }

    }

    public class FooMock
    {
        public string A { get; } = "hello";
        public string B { get; } = "world";
        public int C { get; } = 123;
    }
}