using System.Diagnostics;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Bulkhead;
using Polly.Timeout;
using PollyTestDrive.Client;
using Xunit;
using Xunit.Abstractions;

namespace PollyTestDrive.Tests;

public class BulkHeadTests
{
    private readonly ITestOutputHelper _outputHelper;
    private readonly IServiceCollection _services = new ServiceCollection();
    private readonly ServiceProvider _serviceProvider;
    
    public BulkHeadTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
        ClientStartup.ConfigureServices(_services);
        _serviceProvider = _services.BuildServiceProvider();
    }
    
    [Fact]
    public async Task Maximum_10_Executing_And_1_Queued()
    {
        int requestCount = 0;
        int rejectedRequestCount = 0;
        int completedRequetsCount = 0;
        Stopwatch sw = new();
        sw.Start();
        
        using var bulkheadPolicy = Policy
            .BulkheadAsync(
                maxParallelization: 10, 
                maxQueuingActions: 1, 
                onBulkheadRejectedAsync: context =>
                {
                    _outputHelper.WriteLine($"[{sw.Elapsed}] Bulkhead rejected request");
                    Interlocked.Increment(ref rejectedRequestCount);
                    return Task.CompletedTask;
                });

        _outputHelper.WriteLine($"[{sw.Elapsed}] Bulkhead has {bulkheadPolicy.BulkheadAvailableCount} free execution slots");
        _outputHelper.WriteLine($"[{sw.Elapsed}] Bulkhead has {bulkheadPolicy.QueueAvailableCount} free queue slots");
        
        var tasks = new List<Task>();
        for (int i = 0; i < 20; i++)
        {
            requestCount++;
            tasks.Add(
                bulkheadPolicy.ExecuteAsync(
                    async (cancellationToken) =>
                    {
                        var client = _serviceProvider.GetRequiredService<DelayClient>();
                        await client.Execute(2000, cancellationToken);
                        Interlocked.Increment(ref completedRequetsCount);
                        _outputHelper.WriteLine($"[{sw.Elapsed}] Request completed");
                    },
                    CancellationToken.None)
            );
        }

        _outputHelper.WriteLine($"[{sw.Elapsed}] Created {requestCount} requests");
        
        _outputHelper.WriteLine($"[{sw.Elapsed}] Bulkhead has {bulkheadPolicy.BulkheadAvailableCount} free execution slots");
        _outputHelper.WriteLine($"[{sw.Elapsed}] Bulkhead has {bulkheadPolicy.QueueAvailableCount} free queue slots");
        
        Func<Task> act = () => Task.WhenAll(tasks);

        await act.Should().ThrowExactlyAsync<BulkheadRejectedException>();
        
        _outputHelper.WriteLine($"[{sw.Elapsed}] Bulkhead rejected {rejectedRequestCount} out of {requestCount} total requests");
        _outputHelper.WriteLine($"[{sw.Elapsed}] Completed {completedRequetsCount} out of {requestCount} total requests");
        _outputHelper.WriteLine($"[{sw.Elapsed}] The last request above ^^^^^^ completes 2 secs after the rest as that was the one that was queued");
    }
}