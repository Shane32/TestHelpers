using System.Reflection;

namespace Tests;

public class ServiceTestBase<T> : ServiceTestBase
    where T : class
{
    protected T Service => Services.GetRequiredService<T>();

    public ServiceTestBase()
    {
        ServiceCollection.AddSingleton<T>();
    }
}

public class ServiceTestBase : IDisposable
{
    protected readonly IConfiguration Configuration;
    private readonly ServiceCollection _serviceCollection = new();
    private ServiceProvider? _serviceProvider;

    protected IServiceCollection ServiceCollection => _serviceProvider == null ? _serviceCollection : throw new InvalidOperationException("Cannot configure service collection after initialization");
    protected IServiceProvider Services => _serviceProvider ??= _serviceCollection.BuildServiceProvider();

    public ServiceTestBase()
    {
        Configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddUserSecrets(Assembly.GetExecutingAssembly())
            .AddEnvironmentVariables()
            .Build();
    }

    public virtual void Dispose() => _serviceProvider?.Dispose();
}
