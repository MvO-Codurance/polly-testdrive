using Microsoft.Extensions.DependencyInjection;
using Polly;
using PollyTestDrive.Client;
using Xunit;
using Xunit.Abstractions;

namespace PollyTestDrive.Tests;

public class RetryTests
{
    private readonly ITestOutputHelper _outputHelper;
    private readonly IServiceCollection _services = new ServiceCollection();
    private readonly ServiceProvider _serviceProvider;
    
    public RetryTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
        ClientStartup.ConfigureServices(_services);
        _serviceProvider = _services.BuildServiceProvider();
    }
    
    [Fact]
    public async Task Retry_3_Times()
    {
        int totalAttempts = 0;
        var policy = Policy.Handle<HttpRequestException>().RetryAsync(3, (exception, attempt) =>
        {
            _outputHelper.WriteLine($"Retry {attempt} failed with error: {exception.Message}");
            totalAttempts++;
        });

        _outputHelper.WriteLine("Making initial attempt...");
        await policy.ExecuteAsync(async () =>
        {
            var client = _serviceProvider.GetRequiredService<StatusCodeClient>();
            await client.Execute(500, totalAttempts); 
        });
    }
}