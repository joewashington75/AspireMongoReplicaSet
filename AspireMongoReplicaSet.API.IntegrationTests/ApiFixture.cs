using AspireMongoReplicaSet.Replica;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using Projects;

namespace AspireMongoReplicaSet.API.IntegrationTests;

public class ApiFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly IHost _app;
    private readonly ResourceNotificationService _resourceNotificationService;
    private string? _mongoConnectionString;
    private readonly IResourceBuilder<MongoReplicaSetResource> _mongoReplicaSet;
    private IMongoDatabase? _mongoDatabase;

    public IMongoDatabase MongoDatabase
    {
        get
        {
            if (_mongoDatabase != null)
                return _mongoDatabase;

            var scopedServices = Services.CreateScope().ServiceProvider;
            _mongoDatabase = scopedServices.GetRequiredService<IMongoDatabase>();
            return _mongoDatabase;
        }
    }

    public ApiFixture()
    {
        var options = new DistributedApplicationOptions
        {
            AssemblyName = typeof(ApiFixture).Assembly.FullName,
            DisableDashboard = true
        };

        var appBuilder = DistributedApplication.CreateBuilder(options);

        var username = appBuilder.AddParameter("mongo-user", "admin");
        var password = appBuilder.AddParameter("mongo-password", "admin");

        var mongo = appBuilder
            .AddMongoDB("mongo", null, username, password)
            .WithLifetime(ContainerLifetime.Persistent)
            .WithContainerName("mongo-tests")
            .WithReplicaSet("../AspireMongoReplicaSet.Replica");

        var mongoDb = mongo
            .AddDatabase("Test");

        _mongoReplicaSet = appBuilder
            .AddMongoReplicaSet("mongoDb", mongoDb.Resource);

        appBuilder.AddProject<AspireMongoReplicaSet_API>("aspiremongoreplicaset-api")
            .WithReference(_mongoReplicaSet)
            .WaitFor(_mongoReplicaSet);

        _app = appBuilder.Build();

        _resourceNotificationService = _app.Services.GetRequiredService<ResourceNotificationService>();
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureHostConfiguration(config =>
        {
            var data = new Dictionary<string, string?>
            {
                { "ConnectionStrings:MongoDb", _mongoConnectionString },
            };

            config.AddInMemoryCollection(data);
        });

        return base.CreateHost(builder);
    }

    public async Task InitializeAsync()
    {
        await _app.StartAsync();
        _mongoConnectionString = await _mongoReplicaSet.Resource.ConnectionStringExpression.GetValueAsync(CancellationToken.None);
        await _resourceNotificationService.WaitForResourceAsync(_mongoReplicaSet.Resource.Name, KnownResourceStates.Running);
    }

    public new async Task DisposeAsync()
    {
        await _app.StopAsync();
        await base.DisposeAsync();

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