namespace HrAspire.Web.Client.Services;

using System.Net;
using System.Net.Http.Json;

public static class HttpResponseMessageExtensions
{
    public static async Task<string> GetErrorMessageAsync(this HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException("Cannot get an error message from a successful response HTTP response.");
        }

        if (response.Content.Headers.ContentType?.MediaType == "application/problem+json")
        {
            var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            if (!string.IsNullOrWhiteSpace(problemDetails?.Detail))
            {
                return problemDetails.Detail;
            }
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return "Not found.";
        }

        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            return "You don't have access.";
        }

        return Constants.UnexpectedErrorMessage;
    }
}
