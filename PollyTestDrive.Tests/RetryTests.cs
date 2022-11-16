using System.Diagnostics;
using FluentAssertions;
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
        Stopwatch sw = new();
        sw.Start();
        
        var policy = Policy.Handle<HttpRequestException>()
            .RetryAsync(3, (exception, attempt) =>
            {
                _outputHelper.WriteLine($"[{sw.Elapsed}] Retry {attempt} failed with error: {exception.Message}");
                totalAttempts++;
            });

        _outputHelper.WriteLine($"[{sw.Elapsed}] Making initial attempt...");
        
        Func<Task> act = () => policy.ExecuteAsync(async () =>
            {
                var client = _serviceProvider.GetRequiredService<StatusCodeClient>();
                await client.Execute(500, totalAttempts); 
            });

        await act.Should().ThrowExactlyAsync<HttpRequestException>();
    }
    
    [Fact]
    public async Task Wait_And_Retry_3_Times()
    {
        int totalAttempts = 0;
        Stopwatch sw = new();
        sw.Start();

        var policy = Policy.Handle<HttpRequestException>()
            .WaitAndRetryAsync(new[]
            {
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(3)
            }, 
            (exception, calculatedWaitDuration) =>
            {
                _outputHelper.WriteLine($"[{sw.Elapsed}] Retry after {calculatedWaitDuration} failed with error: {exception.Message}");
                totalAttempts++;
            });

        _outputHelper.WriteLine($"[{sw.Elapsed}] Making initial attempt...");
        
        Func<Task> act = () => policy.ExecuteAsync(async () =>
        {
            var client = _serviceProvider.GetRequiredService<StatusCodeClient>();
            await client.Execute(500, totalAttempts); 
        });

        await act.Should().ThrowExactlyAsync<HttpRequestException>();
    }
}