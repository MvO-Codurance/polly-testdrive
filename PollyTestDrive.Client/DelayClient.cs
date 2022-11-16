namespace PollyTestDrive.Client;

public class DelayClient
{
    private readonly HttpClient _client;

    public DelayClient(HttpClient client)
    {
        _client = client;
    }
    
    public async Task Execute(int millisecondDelay)
    {
        await Execute(millisecondDelay, 0);
    }
    
    public async Task Execute(int millisecondDelay, int attempt)
    {
        await Execute(millisecondDelay, attempt, CancellationToken.None);
    }
    
    public async Task Execute(int millisecondDelay, CancellationToken cancellationToken)
    {
        await Execute(millisecondDelay, 0, cancellationToken);
    }
    
    public async Task Execute(int millisecondDelay, int attempt, CancellationToken cancellationToken)
    {
        _ = await _client.GetStringAsync($"{millisecondDelay}?attempt={attempt}", cancellationToken);
    }
}