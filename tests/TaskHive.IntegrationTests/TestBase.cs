using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace TaskHive.IntegrationTests;

public abstract class TestBase : IDisposable
{
    public required HttpClient Client { get; set; }
    public required IServiceScope ServiceScope { get; set; }
    private bool _disposed;

    protected HttpClient ConfigureClient<TStartup>(WebApplicationFactory<TStartup> factory, Func<HttpClient, HttpClient> configureClient) where TStartup : class
    {
        ServiceScope = factory.Services.CreateScope();
        Client = factory.CreateClient();
        return configureClient(Client);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                Client.Dispose();
                ServiceScope.Dispose();
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
} 