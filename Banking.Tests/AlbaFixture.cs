using Alba;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.Extensions.Hosting;
using System.Net.NetworkInformation;


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
        Postgres = appBuilder.AddPostgres("banking");
        _app = appBuilder.Build();
    }
    public async Task InitializeAsync()
    {
        await _app.StartAsync();
      
        _postgresConnectionString = await Postgres.Resource.GetConnectionStringAsync() ?? throw new Exception("No Connection String");

        var port = Postgres.Resource.PrimaryEndpoint.Port;
        var retries = Enumerable.Range(1, 1000);
        var ipProps = IPGlobalProperties.GetIPGlobalProperties();
       
        var portIsAssigned = false;
        foreach (var retry in retries)
        {
            
            var there = ipProps.GetActiveTcpListeners().Any(c => c.Port == port);
            if (there)
            {
                portIsAssigned = true;
                break;
            }
            await Task.Delay(1);
            
        }
        if(!portIsAssigned)
        {
            throw new ArgumentOutOfRangeException("The Port isn't available");
        }
        await Task.Delay(1000);
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
