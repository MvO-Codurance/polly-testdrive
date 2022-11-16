using Microsoft.Extensions.DependencyInjection;

namespace PollyTestDrive.Client;

public static class ClientStartup
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpClient<StatusCodeClient>(client =>
        {
            client.BaseAddress = new Uri("http://localhost:5296/status-code/");
        });
        
        services.AddHttpClient<DelayClient>(client =>
        {
            client.BaseAddress = new Uri("http://localhost:5296/delay/");
        });
    }
}