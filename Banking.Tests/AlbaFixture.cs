using Alba;
using Microsoft.Extensions.Hosting;


namespace Banking.Tests;
public class AlbaFixture : IAsyncLifetime
{
    public IAlbaHost Host = null!;
    private readonly IHost _app;
    public IResourceBuilder<PostgresServerResource> Postgres { get; private set; }
    private string _postgresConnectionString = null!;

    public AlbaFixture()
    {
        var options = new DistributedApplicationOptions { AssemblyName = typeof(AlbaFixture).Assembly.FullName, DisableDashboard = true };
        var appBuilder = DistributedApplication.CreateBuilder(options);
        Postgres = appBuilder.AddPostgres("db");
        _app = appBuilder.Build();
    }
    public async Task InitializeAsync()
    {
        await _app.StartAsync();
        await Task.Delay(1000); // YUCK - Need a readiness check?
        _postgresConnectionString = await Postgres.Resource.GetConnectionStringAsync() ?? throw new Exception("No Connection String");

        Host = await AlbaHost.For<global::Program>(config =>
        {
            config.UseSetting("ConnectionStrings:banking", _postgresConnectionString);
        });
    }
    public async Task DisposeAsync()
    {
        await _app.StopAsync();
        if (_app is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync().ConfigureAwait(false);
        }
        else
        {
            _app.Dispose();
        }
    }
}
