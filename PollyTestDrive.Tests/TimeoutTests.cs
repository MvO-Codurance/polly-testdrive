using System.Diagnostics;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Timeout;
using PollyTestDrive.Client;
using Xunit;
using Xunit.Abstractions;

namespace PollyTestDrive.Tests;

public class TimeoutTests
{
    private readonly ITestOutputHelper _outputHelper;
    private readonly IServiceCollection _services = new ServiceCollection();
    private readonly ServiceProvider _serviceProvider;
    
    public TimeoutTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
        ClientStartup.ConfigureServices(_services);
        _serviceProvider = _services.BuildServiceProvider();
    }
    
    [Fact]
    public async Task Optimistic_Timeout_After_3_Seconds()
    {
        Stopwatch sw = new();
        sw.Start();

        // use TimeoutStrategy.Optimistic where the delegate supports co-operative cancellation 
        
        var policy = Policy
            .TimeoutAsync(
                timeout: TimeSpan.FromSeconds(3),
                onTimeoutAsync: (context, timespan, task) =>
                {
                    _outputHelper.WriteLine($"[{sw.Elapsed}] Timed out after {timespan}");
                    return Task.CompletedTask;
                }, 
                timeoutStrategy: TimeoutStrategy.Optimistic);

        _outputHelper.WriteLine($"[{sw.Elapsed}] Making initial attempt...");
        
        Func<Task> act = () => policy.ExecuteAsync(
            async (cancellationToken) =>
            {
                var client = _serviceProvider.GetRequiredService<DelayClient>();
                await client.Execute(5000, cancellationToken); 
            },
            CancellationToken.None);

        await act.Should().ThrowExactlyAsync<TimeoutRejectedException>();
    }
    
    [Fact]
    public async Task Pessimistic_Timeout_After_3_Seconds()
    {
        Stopwatch sw = new();
        sw.Start();

        // use TimeoutStrategy.Pessimistic where the delegate has no in-built timeout and do not honor cancellation
        
        var policy = Policy
            .TimeoutAsync(
                timeout: TimeSpan.FromSeconds(3),
                onTimeoutAsync: (context, timespan, task) =>
                {
                    _outputHelper.WriteLine($"[{sw.Elapsed}] Timed out after {timespan}");
                    return Task.CompletedTask;
                }, 
                timeoutStrategy: TimeoutStrategy.Pessimistic);

        _outputHelper.WriteLine($"[{sw.Elapsed}] Making initial attempt...");
        
        Func<Task> act = () => policy.ExecuteAsync(
            async () =>
            {
                var client = _serviceProvider.GetRequiredService<DelayClient>();
                await client.Execute(5000); 
            });

        await act.Should().ThrowExactlyAsync<TimeoutRejectedException>();
    }
}