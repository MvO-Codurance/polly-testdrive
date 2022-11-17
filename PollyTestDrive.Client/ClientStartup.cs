using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Contrib.TimingPolicy;

namespace PollyTestDrive.Client;

public static class ClientStartup
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging();
        
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
        
        services.AddHttpClient<DelayClientUsingHttpClientFactory>(client =>
            {
                client.BaseAddress = new Uri("http://localhost:5296/delay/");
            })
            .AddPolicyHandler((serviceProvider, request) => 
                AsyncTimingPolicy<HttpResponseMessage>.Create((TimeSpan executionDuration, Context context) =>
                {
                    var logger = serviceProvider.GetRequiredService<ILogger<DelayClientUsingHttpClientFactory>>();
                    logger.LogInformation($"Execution duration: {executionDuration} for url {request.RequestUri}. CorrelationId: {context.CorrelationId}");
                    return Task.CompletedTask;
                }));
    }
}