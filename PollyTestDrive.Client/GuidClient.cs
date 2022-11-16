namespace PollyTestDrive.Client;

public class GuidClient
{
    private readonly HttpClient _client;

    public GuidClient(HttpClient client)
    {
        _client = client;
    }
    
    public async Task<string> Execute()
    {
        return await _client.GetStringAsync(string.Empty);
    }
}