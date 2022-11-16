namespace PollyTestDrive.Client;

public class StatusCodeClient
{
    private readonly HttpClient _client;

    public StatusCodeClient(HttpClient client)
    {
        _client = client;
    }
    
    public async Task Execute(int statusCode, int attempt)
    {
        _ = await _client.GetStringAsync($"{statusCode}?attempt={attempt}");
    }
}