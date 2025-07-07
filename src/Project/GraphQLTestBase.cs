using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Shane32.TestHelpers;

/// <inheritdoc/>
public abstract class GraphQLTestBase<TStartup> : GraphQLTestBase
    where TStartup : class
{
    /// <inheritdoc/>
    protected override void ConfigureWebHostBuilder(IWebHostBuilder webHostBuilder)
    {
        base.ConfigureWebHostBuilder(webHostBuilder);
        webHostBuilder.UseStartup<TStartup>();
    }
}

/// <summary>
/// Represents the base class for GraphQL testing against a test server.
/// </summary>
public abstract class GraphQLTestBase : IDisposable, Xunit.IAsyncLifetime, IAsyncDisposable
{
    /// <summary>
    /// The name of the test environment.
    /// </summary>
    public const string TestEnvironmentName = "Test";

    private readonly ServiceCollection _serviceCollection = new();
    /// <summary>
    /// Gets the service collection that will be used to configure the test server.
    /// </summary>
    protected IServiceCollection ServiceCollection
        => _webHost == null
            ? _serviceCollection
            : throw new InvalidOperationException("Cannot configure services after service provider has been initialized; please move all mocks prior to any call to Service, Services, or Db.");

    private IServiceProvider? _services;
    /// <summary>
    /// Gets the service provider that will be used to resolve services during the test.
    /// </summary>
    protected IServiceProvider Services => _services ??= WebHost.Services;

    private IWebHost? _webHost;
    /// <summary>
    /// Gets or sets the test server that will be used to execute GraphQL queries.
    /// When setting this property, provide a test server that has already been started.
    /// </summary>
    protected IWebHost WebHost
    {
        get {
            if (_webHost == null) {
                var webHostBuilder = new WebHostBuilder();
                ConfigureWebHostBuilder(webHostBuilder);
                _webHost = webHostBuilder.Build();
                _webHost.Start();
            }
            return _webHost;
        }
        set {
            if (_webHost != null)
                throw new InvalidOperationException("Cannot set WebHost after it has been initialized");
            _webHost = value;
        }
    }

    private HttpClient? _client;
    /// <summary>
    /// Gets the HTTP client that will be used to execute GraphQL queries against the test server.
    /// </summary>
    protected HttpClient Client => _client ??= WebHost.GetTestClient();

    /// <summary>
    /// Gets or sets the claims that will be used to create the access token for the GraphQL queries.
    /// Defaults to a claim for the "aud" (audience) with the value "TestAudience", and a claim for the "iss" (issuer) with the value "TestIssuer".
    /// Clearing the claims list will result in no access token being sent with the GraphQL queries.
    /// </summary>
    public ClaimsList Claims { get; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GraphQLTestBase"/> class.
    /// </summary>
    public GraphQLTestBase()
    {
        Claims.Set(JwtRegisteredClaimNames.Aud, "TestAudience");
        Claims.Set(JwtRegisteredClaimNames.Iss, "TestIssuer");
    }

    private static readonly ConcurrentDictionary<Assembly, IConfiguration?> _serverConfigFiles = new();
    /// <summary>
    /// Gets the server configuration JSON from the specified assembly.
    /// </summary>
    protected virtual IConfiguration? GetServerConfig()
    {
        var assembly = GetServerConfigAssembly();
        return _serverConfigFiles.GetOrAdd(assembly, assembly2 => {
            //get all resource names from assembly, and find the one that ends with ".ServerConfig.json" or equals "ServerConfig.json" (case insensitive)
            var resourceName = GetServerConfigResourceName(assembly2);
            if (resourceName == null)
                return null;
            using var stream = assembly2.GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException($"Resource {resourceName} was not found in assembly {assembly2.FullName}");
            var configuration = new ConfigurationBuilder()
                .AddJsonStream(stream)
                .Build();
            return configuration;
        });
    }

    /// <summary>
    /// Gets the assembly that contains the server configuration JSON.
    /// </summary>
    /// <returns></returns>
    protected virtual Assembly GetServerConfigAssembly() => GetType().Assembly;

    /// <summary>
    /// Gets the name of the server configuration JSON resource.
    /// </summary>
    protected virtual string? GetServerConfigResourceName(Assembly assembly)
        => assembly.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith(".ServerConfig.json", StringComparison.OrdinalIgnoreCase) || n.Equals("ServerConfig.json", StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Builds the <see cref="WebHostBuilder"/> that will be used to create the test server.
    /// Call <see cref="IWebHostBuilder"/>.<see cref="Microsoft.AspNetCore.TestHost.WebHostBuilderExtensions.ConfigureTestServices(IWebHostBuilder, Action{IServiceCollection})">ConfigureTestServices</see>
    /// to override services from your Startup with test services.
    /// </summary>
    protected virtual void ConfigureWebHostBuilder(IWebHostBuilder webHostBuilder)
    {
        webHostBuilder
            .UseTestServer()
            .ConfigureAppConfiguration((context, config) => {
                // configure the environment name for identification later
                context.HostingEnvironment.EnvironmentName = TestEnvironmentName;
                // clear any default sources (e.g. environment variables)
                config.Sources.Clear();
                // add the test configuration file (named serverconfig.json) from the test class's assembly
                var serverConfig = GetServerConfig();
                if (serverConfig != null) {
                    config.Add(new ChainedConfigurationSource() {
                        Configuration = serverConfig,
                        ShouldDisposeConfiguration = false,
                    });
                }
            })
            // ensure that IConfiguration is available in the service provider
            .ConfigureServices((context, services) => services.AddSingleton(context.Configuration))
            // configure test services
            .ConfigureTestServices(services => {
                // configure the JWT bearer tokens for the test server
                services.ConfigureUnsignedJwtBearerTokens();
                // configure any services manually added by the test
                foreach (var service in _serviceCollection) {
                    services.Add(service);
                }
            });
    }

    /// <summary>
    /// Gets the path to the GraphQL endpoint.
    /// </summary>
    protected virtual string GetGraphQLPath() => "/api/graphql";

    /// <summary>
    /// Runs the specified GraphQL query and returns the response.
    /// </summary>
    /// <param name="query">The GraphQL query to execute.</param>
    /// <param name="variables">Variables in the form of either a JSON string or an object.</param>
    /// <exception cref="InvalidOperationException"></exception>
    public virtual async Task<ExecutionResponse> RunQueryAsync(string query, object? variables = null)
    {
        // note: sending as POST encoded as JSON so CSRF protection is not triggered
        var httpClient = Client;
        using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, GetGraphQLPath());
        var request = new {
            query,
            variables = variables is string variablesString ? JsonSerializer.Deserialize(variablesString, typeof(JsonElement)) : variables
        };
        httpRequestMessage.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        if (Claims.Count > 0)
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await GetAccessTokenAsync().ConfigureAwait(false));
        using var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);
        if (httpResponseMessage.StatusCode != HttpStatusCode.OK && httpResponseMessage.StatusCode != HttpStatusCode.BadRequest)
            throw new InvalidOperationException($"GraphQL request failed with status code {httpResponseMessage.StatusCode}");
        var responseStream = await httpResponseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false);
        var response = JsonSerializer.Deserialize<ExecutionResponse>(responseStream)
            ?? throw new InvalidOperationException("Null was received from GraphQL server");
        response.StatusCode = httpResponseMessage.StatusCode;
        return response;
    }

    /// <summary>
    /// Gets the list of claims that will be used to create the access token for the GraphQL queries.
    /// </summary>
    protected virtual Task<IEnumerable<Claim>> GetClaimsAsync() => Task.FromResult<IEnumerable<Claim>>(Claims);

    /// <summary>
    /// Creates a JWT access token based on the <see cref="Claims"/>.
    /// </summary>
    protected virtual async Task<string> GetAccessTokenAsync()
    {
        var now = DateTime.UtcNow;
        var descriptor = new SecurityTokenDescriptor();
        descriptor.Expires = now.AddMinutes(5);
        descriptor.NotBefore = now;
        descriptor.IssuedAt = now;
        descriptor.Subject = new ClaimsIdentity(await GetClaimsAsync().ConfigureAwait(false));
        var handler = new JsonWebTokenHandler();
        var token = handler.CreateToken(descriptor);
        return token;
    }

    /// <inheritdoc/>
    public virtual void Dispose()
    {
        _client?.Dispose();
        _webHost?.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc cref="Xunit.IAsyncLifetime.InitializeAsync"/>
    protected virtual Task InitializeAsync() => Task.CompletedTask;

    Task Xunit.IAsyncLifetime.InitializeAsync() => InitializeAsync();
    Task Xunit.IAsyncLifetime.DisposeAsync() => DisposeAsync().AsTask();

    /// <inheritdoc cref="IAsyncDisposable.DisposeAsync"/>
    public virtual async ValueTask DisposeAsync()
    {
        _client?.Dispose();
        if (_webHost is IAsyncDisposable asyncDisposable) {
            await asyncDisposable.DisposeAsync().ConfigureAwait(false);
        } else {
            _webHost?.Dispose();
        }
        GC.SuppressFinalize(this);
    }
}
