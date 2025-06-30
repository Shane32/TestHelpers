using GraphQL;
using GraphQL.Types;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;

namespace Tests;

public class GraphQLTestBaseTests : GraphQLTestBase<GraphQLTestBaseTests.TestStartup>
{
    [Fact]
    public async Task HelloQuery_ShouldReturnWorld()
    {
        // Arrange
        string query = "{ hello }";

        // Act
        var response = await RunQueryAsync(query);

        // Assert
        response.ShouldBeSuccessful();
        response.ShouldBeSimilarToJson("""{"data":{"hello":"world"}}""");
    }

    [Fact]
    public async Task InvalidQuery_ShouldReturnError()
    {
        // Arrange
        string invalidQuery = "{ invalidField }";

        // Act
        var response = await RunQueryAsync(invalidQuery);

        // Assert
        response.ShouldMatchApproved();
    }

    [Fact]
    public async Task UnauthorizedAccess_ShouldFail()
    {
        // Arrange
        string query = "{ hello }";

        // Clear claims to simulate unauthorized access
        Claims.Clear();

        // Act
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            async () => await RunQueryAsync(query));

        // Assert
        exception.Message.ShouldContain("GraphQL request failed with status code Unauthorized");
    }

    public class TestStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(o => {
                    o.Authority = "https://api.example.com";
                    o.Audience = "Sample";
                });
            services.AddGraphQL(b => b
                .AddSchema<HelloSchema>()
                .AddSystemTextJson()
                .AddGraphTypes(typeof(HelloQuery).Assembly)
            );
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseAuthentication();
            app.UseGraphQL<ISchema>("/api/graphql", o => o.AuthorizationRequired = true);
        }
    }

    public class HelloSchema : Schema
    {
        public HelloSchema(IServiceProvider provider) : base(provider)
        {
            Query = provider.GetRequiredService<HelloQuery>();
        }
    }

    public class HelloQuery : ObjectGraphType
    {
        public HelloQuery()
        {
            Field<StringGraphType>("hello")
                .Resolve(context => "world");
        }
    }

}
