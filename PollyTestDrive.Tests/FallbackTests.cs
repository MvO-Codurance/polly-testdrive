using System.Diagnostics;
using System.Globalization;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Caching.Memory;
using Polly.RateLimit;
using PollyTestDrive.Client;
using Xunit;
using Xunit.Abstractions;

namespace PollyTestDrive.Tests;

public class FallbackTests
{
    private readonly ITestOutputHelper _outputHelper;
    private readonly IServiceCollection _services = new ServiceCollection();
    private readonly ServiceProvider _serviceProvider;
    
    public FallbackTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
        ClientStartup.ConfigureServices(_services);
        _serviceProvider = _services.BuildServiceProvider();
    }
    
    [Fact]
    public async Task Fallback_To_Empty_Guid()
    {
        Stopwatch sw = new();
        sw.Start();

        var fallbackPolicy = Policy
            .Handle<Exception>()
            .OrResult(result: string.Empty)
            .FallbackAsync(
                fallbackValue: Guid.Empty.ToString(), 
                onFallbackAsync: (result, context) =>
                {
                    _outputHelper.WriteLine($"[{sw.Elapsed}] Using fallback value due to {result.Exception.Message}");
                    return Task.CompletedTask;
                });

        for (int i = 0; i < 10; i++)
        {
            // fail even numbered attempts
            bool fail = i % 2 == 0;
            
            string result = await fallbackPolicy
                .ExecuteAsync(async () =>
                {
                    if (fail)
                    {
                        var statusCodeClient = _serviceProvider.GetRequiredService<StatusCodeClient>();
                        await statusCodeClient.Execute(500, 0);    
                    }
                    
                    var guidClient = _serviceProvider.GetRequiredService<GuidClient>();
                    return await guidClient.Execute();
                });
            
            _outputHelper.WriteLine(message: $"[{sw.Elapsed}] Attempt {i}, response is {result}");
        }
    }
}