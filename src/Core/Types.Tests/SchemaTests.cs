using System.Collections.Generic;
using System.Threading.Tasks;
using ChilliCream.Testing;
using HotChocolate.Execution;
using Xunit;

namespace HotChocolate
{
    public class SchemaTests
    {
        [Fact]
        public async Task Foo()
        {
            string source = "type Query { foo: Foo list: [Foo] } type Foo { a: String }";
            ISchema schema = Schema.Create(source, ctx =>
            {
                ctx.Use(next => context =>
                {
                    var parent = context.Parent<Dictionary<string, object>>();

                    if (parent.TryGetValue(context.Field.Name, out object value))
                    {
                        context.Result = value;
                    }
                    else
                    {
                        context.Result = null;
                    }

                    return Task.CompletedTask;
                });
            });

            string query = "{ foo { a } }";
            var dict = new Dictionary<string, object>
            {
                { "foo", new Dictionary<string, object> { { "a", "bar"} } }
            };

            IQueryExecuter executer = schema.MakeExecutable();
            IExecutionResult result = await executer.ExecuteAsync(new QueryRequest(query)
            {
                InitialValue = dict
            });

            result.Snapshot();
        }
    }
}
