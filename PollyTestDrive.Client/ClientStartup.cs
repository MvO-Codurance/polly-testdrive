using Microsoft.Extensions.DependencyInjection;
using Polly;

namespace PollyTestDrive.Client;

public static class ClientStartup
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpClient<StatusCodeClient>(client => client.BaseAddress = new Uri("http://localhost:5296/status-code/"));
        services.AddHttpClient<DelayClient>(client => client.BaseAddress = new Uri("http://localhost:5296/delay/"));
        services.AddHttpClient<GuidClient>(client => client.BaseAddress = new Uri("http://localhost:5296/guid/"));
        
        services.AddHttpClient<StatusCodeClientUsingHttpClientFactory>(client =>
        {
            client.BaseAddress = new Uri("http://localhost:5296/status-code/");
        })
        .AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryAsync(new[]
        {
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(2),
            TimeSpan.FromSeconds(3)
        }));
    }
}