using System.Diagnostics;
using System.Drawing;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.CircuitBreaker;
using PollyTestDrive.Client;
using Xunit;
using Xunit.Abstractions;

namespace PollyTestDrive.Tests;

public class CircuitBreakerTests
{
    private readonly ITestOutputHelper _outputHelper;
    private readonly IServiceCollection _services = new ServiceCollection();
    private readonly ServiceProvider _serviceProvider;
    
    public CircuitBreakerTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
        ClientStartup.ConfigureServices(_services);
        _serviceProvider = _services.BuildServiceProvider();
    }
    
    [Fact]
    public async Task Break_After_4_Attempts()
    {
        int totalAttempts = 0;
        Stopwatch sw = new();
        sw.Start();
        
        var waitAndRetryPolicy = Policy
            .Handle<Exception>(e => !(e is BrokenCircuitException)) // We don't retry if the inner circuit-breaker judges the underlying system is out of commission!
            .WaitAndRetryForeverAsync(
                sleepDurationProvider: _ => TimeSpan.FromMilliseconds(500),
                onRetry: (exception, calculatedWaitDuration) =>
                {
                    _outputHelper.WriteLine($"[{sw.Elapsed}] Retry {++totalAttempts} after {calculatedWaitDuration} failed with error: {exception.Message}");
                });
        
        var breakerPolicy = Policy
            .Handle<HttpRequestException>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 4, 
                durationOfBreak: TimeSpan.FromSeconds(2), 
                onBreak: (exception, breakDelay) => _outputHelper.WriteLine($"[{sw.Elapsed}] Breaking the circuit for {breakDelay.TotalSeconds} secs due to error: {exception.Message}"),
                onReset: () => _outputHelper.WriteLine($"[{sw.Elapsed}] Circuit breaker reset"),
                onHalfOpen: () => _outputHelper.WriteLine($"[{sw.Elapsed}] Circuit breaker half-open, next call is a trial"));

        var policyWrap = Policy.WrapAsync(waitAndRetryPolicy, breakerPolicy);
        
        _outputHelper.WriteLine($"[{sw.Elapsed}] Making initial attempt...");
        
        Func<Task> act = () => policyWrap.ExecuteAsync(async () =>
            {
                var client = _serviceProvider.GetRequiredService<StatusCodeClient>();
                await client.Execute(500, totalAttempts); 
            });

        await act.Should().ThrowExactlyAsync<BrokenCircuitException>();
    }
}