using System.Diagnostics;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using PollyTestDrive.Client;
using Xunit;
using Xunit.Abstractions;

namespace PollyTestDrive.Tests;

public class StatusCodeClientUsingHttpClientFactoryTests
{
    private readonly ITestOutputHelper _outputHelper;
    private readonly IServiceCollection _services = new ServiceCollection();
    private readonly ServiceProvider _serviceProvider;
    
    public StatusCodeClientUsingHttpClientFactoryTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
        ClientStartup.ConfigureServices(_services);
        _serviceProvider = _services.BuildServiceProvider();
    }
    
    [Fact]
    public async Task Call_Failing_Method_That_Internally_Handles_Retries()
    {
        Stopwatch sw = new();
        sw.Start();
        
        _outputHelper.WriteLine($"[{sw.Elapsed}] Making initial attempt...");
        
        Func<Task> act = async () => 
        {
            try
            {
                var client = _serviceProvider.GetRequiredService<StatusCodeClientUsingHttpClientFactory>();
                await client.Execute(500, 0);
            }
            catch (Exception ex)
            {
                _outputHelper.WriteLine($"[{sw.Elapsed}] Attempt failed due to {ex.Message}");
                throw;
            }
        };

        await act.Should().ThrowExactlyAsync<HttpRequestException>();
    }
}