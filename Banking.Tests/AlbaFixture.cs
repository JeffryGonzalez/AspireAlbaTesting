using Alba;
using Microsoft.Extensions.Hosting;
using System.Data.Common;


namespace Banking.Tests;
public class AlbaFixture : IAsyncLifetime
{
    public IAlbaHost Host = null!;
    private readonly IHost _app;
    public IResourceBuilder<PostgresServerResource> Postgres { get; private set; }
    private string _postgresConnectionString = null!;
    private IDistributedApplicationBuilder _appBuilder;
    public AlbaFixture()
    {
        var options = new DistributedApplicationOptions { AssemblyName = typeof(AlbaFixture).Assembly.FullName, DisableDashboard = true };
        _appBuilder = DistributedApplication.CreateBuilder(options);
        Postgres = _appBuilder.AddPostgres("banking");
        _app = _appBuilder.Build();
    }
    public async Task InitializeAsync()
    {
        await _app.StartAsync();

        _postgresConnectionString = await Postgres.Resource.GetConnectionStringAsync() ?? throw new Exception("No Connection String");


        await Postgres.EnsureConnectionAsync();

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

public static class ProbePostgresConnectionExtensions
{
    public static async Task EnsureConnectionAsync(this IResourceBuilder<PostgresServerResource> resource, int timeOut = 3, int retries = 3, CancellationToken token = default)
    {
        var connectionString = await resource.Resource.GetConnectionStringAsync() ?? throw new ArgumentOutOfRangeException("Cannot get connection string");

        connectionString = connectionString + $";Timeout={timeOut}";

        var probeConnection = new Npgsql.NpgsqlConnection(connectionString);

        while (true)
        {
            var ctr = 0;
            try
            {
                await probeConnection.OpenAsync(token);
                break;
            }
            catch (DbException)
            {
                if (++ctr == retries) { throw; }
            }
        }
    }
}