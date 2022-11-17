using System.Diagnostics;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.RateLimit;
using PollyTestDrive.Client;
using Xunit;
using Xunit.Abstractions;

namespace PollyTestDrive.Tests;

public class RateLimitTests
{
    private readonly ITestOutputHelper _outputHelper;
    private readonly IServiceCollection _services = new ServiceCollection();
    private readonly ServiceProvider _serviceProvider;
    
    public RateLimitTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
        ClientStartup.ConfigureServices(_services);
        _serviceProvider = _services.BuildServiceProvider();
    }
    
    [Fact]
    public async Task Maximum_10_Executing_In_1_Second()
    {
        int requestCount = 0;
        int rejectedRequestCount = 0;
        int completedRequetsCount = 0;
        Stopwatch sw = new();
        sw.Start();
        
        var rateLimitPolicy = Policy
            .RateLimitAsync(
                numberOfExecutions: 10, 
                perTimeSpan: TimeSpan.FromSeconds(1), 
                maxBurst: 10, // to allow 10 requests to be started at once
                retryAfterFactory: (retryAfter, context) =>
                {
                    _outputHelper.WriteLine($"[{sw.Elapsed}] Rate limit rejected request, should retry after {retryAfter}");
                    Interlocked.Increment(ref rejectedRequestCount);
                    return Task.CompletedTask;
                });
        
        var tasks = new List<Task>();
        for (int i = 0; i < 20; i++)
        {
            requestCount++;
            tasks.Add(
                rateLimitPolicy.ExecuteAsync(
                    async (cancellationToken) =>
                    {
                        var client = _serviceProvider.GetRequiredService<DelayClient>();
                        await client.Execute(2000, cancellationToken);
                        Interlocked.Increment(ref completedRequetsCount);
                        _outputHelper.WriteLine($"[{sw.Elapsed}] Request completed");
                        return Task.CompletedTask;
                    },
                    CancellationToken.None)
            );
        }

        _outputHelper.WriteLine($"[{sw.Elapsed}] Created {requestCount} requests");
        
        Func<Task> act = () => Task.WhenAll(tasks);

        await act.Should().NotThrowAsync();
        
        _outputHelper.WriteLine($"[{sw.Elapsed}] Rate limit rejected {rejectedRequestCount} out of {requestCount} total requests");
        _outputHelper.WriteLine($"[{sw.Elapsed}] Completed {completedRequetsCount} out of {requestCount} total requests");
    }
    
    [Fact]
    public async Task Maximum_10_Executing_In_1_Second_Add_5_Every_300_Milliseconds()
    {
        int requestCount = 0;
        int rejectedRequestCount = 0;
        int completedRequetsCount = 0;
        Stopwatch sw = new();
        sw.Start();
        
        var rateLimitPolicy = Policy
            .RateLimitAsync(
                numberOfExecutions: 10, 
                perTimeSpan: TimeSpan.FromSeconds(1), 
                maxBurst: 5, // to allow 5 requests to be started at once
                retryAfterFactory: (retryAfter, context) =>
                {
                    _outputHelper.WriteLine($"[{sw.Elapsed}] Rate limit rejected request, should retry after {retryAfter}");
                    Interlocked.Increment(ref rejectedRequestCount);
                    return Task.CompletedTask;
                });
        
        var tasks = new List<Task>();

        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                requestCount++;
                tasks.Add(
                    rateLimitPolicy.ExecuteAsync(
                        async (cancellationToken) =>
                        {
                            var client = _serviceProvider.GetRequiredService<DelayClient>();
                            await client.Execute(1000, cancellationToken);
                            Interlocked.Increment(ref completedRequetsCount);
                            _outputHelper.WriteLine($"[{sw.Elapsed}] Request completed");
                            return Task.CompletedTask;
                        },
                        CancellationToken.None)
                );
            }
            
            await Task.Delay(300);
        }

        _outputHelper.WriteLine($"[{sw.Elapsed}] Created {requestCount} requests");
        
        Func<Task> act = () => Task.WhenAll(tasks);

        await act.Should().NotThrowAsync();
        
        _outputHelper.WriteLine($"[{sw.Elapsed}] Rate limit rejected {rejectedRequestCount} out of {requestCount} total requests");
        _outputHelper.WriteLine($"[{sw.Elapsed}] Completed {completedRequetsCount} out of {requestCount} total requests");
    }
}