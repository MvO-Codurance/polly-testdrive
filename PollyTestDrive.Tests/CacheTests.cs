using System.Diagnostics;
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

public class CacheTests
{
    private readonly ITestOutputHelper _outputHelper;
    private readonly IServiceCollection _services = new ServiceCollection();
    private readonly ServiceProvider _serviceProvider;
    
    public CacheTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
        ClientStartup.ConfigureServices(_services);
        _serviceProvider = _services.BuildServiceProvider();
    }
    
    [Fact]
    public async Task Cache_Guid_For_3_Seconds()
    {
        Stopwatch sw = new();
        sw.Start();
        
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var memoryCacheProvider = new MemoryCacheProvider(memoryCache);
        var cachePolicy = Policy
            .CacheAsync<string>(
                cacheProvider: memoryCacheProvider, 
                ttl: TimeSpan.FromSeconds(3),
                onCacheError: (context, key, exception) => {
                    _outputHelper.WriteLine($"[{sw.Elapsed}]Cache provider for key {key} errored {exception.Message}");
                });

        for (int i = 0; i < 12; i++)
        {
            string result = await cachePolicy
                .ExecuteAsync(async context =>
                    {
                        var client = _serviceProvider.GetRequiredService<GuidClient>();
                        return await client.Execute();
                    }, 
                    new Context("MyGuid"));
            
            _outputHelper.WriteLine($"[{sw.Elapsed}] Guid response {result}");

            await Task.Delay(500);
        }
    }
}