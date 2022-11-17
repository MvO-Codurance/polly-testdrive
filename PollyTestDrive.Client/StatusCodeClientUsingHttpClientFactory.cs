namespace PollyTestDrive.Client;

public class StatusCodeClientUsingHttpClientFactory
{
    private readonly HttpClient _client;

    public StatusCodeClientUsingHttpClientFactory(HttpClient client)
    {
        _client = client;
    }
    
    public async Task Execute(int statusCode, int attempt)
    {
        _ = await _client.GetStringAsync($"{statusCode}?attempt={attempt}");
    }
}