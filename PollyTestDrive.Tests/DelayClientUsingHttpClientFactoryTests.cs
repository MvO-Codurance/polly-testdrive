using System.Diagnostics;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using PollyTestDrive.Client;
using Xunit;
using Xunit.Abstractions;

namespace PollyTestDrive.Tests;

public class DelayClientUsingHttpClientFactoryTests
{
    private readonly ITestOutputHelper _outputHelper;
    private readonly IServiceCollection _services = new ServiceCollection();
    private readonly ServiceProvider _serviceProvider;
    
    public DelayClientUsingHttpClientFactoryTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
        _services.AddLogging((builder) => builder.AddXUnit(_outputHelper));
        ClientStartup.ConfigureServices(_services);
        _serviceProvider = _services.BuildServiceProvider();
    }
    
    [Fact]
    public async Task Log_Execution_Timings()
    {
        Stopwatch sw = new();
        sw.Start();
        
        _outputHelper.WriteLine($"[{sw.Elapsed}] Making initial attempt...");
        
        Func<Task> act = async () => 
        {
            var client = _serviceProvider.GetRequiredService<DelayClientUsingHttpClientFactory>();
            await client.Execute(2000);
        };

        await act.Should().NotThrowAsync();
    }
}