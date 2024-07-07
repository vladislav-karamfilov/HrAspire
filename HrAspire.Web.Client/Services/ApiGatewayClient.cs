namespace HrAspire.Web.Services;

public class ApiGatewayClient
{
    private readonly HttpClient httpClient;

    public ApiGatewayClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public Task<string> GetWeatherAsync()
    {
        return this.httpClient.GetStringAsync("/weatherforecast");
    }
}
