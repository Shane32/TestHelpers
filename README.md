# Shane32.TestHelpers

This library contains a set of helper classes and methods to facilitate the testing of GraphQL applications.

## GraphQLTestBase

This class is a base class for testing GraphQL queries and mutations.
Use the generic parameter to specify the startup class of the application to be tested.
If a `serverconfig.json` file is found as an embedded resource within the test project, it will be used
to configure the test server as if it was an `appsettings.json` file.

Sample use with SQLite EF Core database:

```cs
/// <summary>
/// Represents the base class for GraphQL testing.
/// </summary>
public class TestBase : GraphQLTestBase<Startup>
{
    public TestBase() : base()
    {
        // set the default role
        Claims.Set("role", "All.Admin");
    }

    /// <summary>
    /// Gets the test database context.
    /// </summary>
    protected TestDbContext Db => (TestDbContext)Services.GetRequiredService<AppDbContext>();

    /// <inheritdoc/>
    protected override void ConfigureWebHostBuilder(IWebHostBuilder webHostBuilder)
    {
        // prep environment:
        //   set environment name to "Test"
        //   use the serverconfig.json embedded resource as the configuration source, if found
        //   run the Startup class's Configure and ConfigureServices methods
        //   allow unsigned JWT tokens and do not verify the issuer or audience
        base.ConfigureWebHostBuilder(webHostBuilder);

        // apply additional configuration below, which applies after the base configuration
        webHostBuilder
            .ConfigureTestServices(services => {
                // reconfigure database connection to use the SQLite in-memory database
                services.AddSingleton(_ => {
                    var c = new SqliteConnection("Data Source=:memory:");
                    c.Open();
                    return c;
                });
                services.AddSingleton<AppDbContext>(provider => {
                    var db = new TestDbContext(provider.GetRequiredService<SqliteConnection>());
                    db.Database.EnsureCreated();
                    db.ResetDbTrace();
                    return db;
                });
            });
    }

    /// <inheritdoc/>
    public override Task<ExecutionResponse> RunQueryAsync(string query, object? variables = null) => RunQueryAsync(query, variables, true);

    /// <inheritdoc cref="RunQueryAsync(string, object?)"/>
    /// <param name="noTracking">Whether to clear the change tracker before and after running the query.</param>
    public async Task<ExecutionResponse> RunQueryAsync(string query, object? variables = null, bool noTracking = true)
    {
        if (noTracking)
            Db.ChangeTracker.Clear();
        var response = await base.RunQueryAsync(query, variables);
        if (noTracking)
            Db.ChangeTracker.Clear();
        return response;
    }
}
```

Sample test:

```cs
public class Class1 : TestBase
{
    [Fact]
    public async Task Typename()
    {
        var response = await RunQueryAsync("""
            query {
              __typename
            }
            """);
        response.ShouldMatchApproved();
    }
}
```

## Credits

Glory to Jehovah, Lord of Lords and King of Kings, creator of Heaven and Earth, who through his Son Jesus Christ,
has reedemed me to become a child of God. -Shane32
