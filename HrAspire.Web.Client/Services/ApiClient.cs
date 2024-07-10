namespace HrAspire.Web.Client.Services;

public class ApiClient
{
    private readonly HttpClient httpClient;

    public ApiClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public Task<string> GetWeatherAsync()
    {
        return httpClient.GetStringAsync("/weatherforecast");
    }
}
