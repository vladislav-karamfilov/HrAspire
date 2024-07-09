namespace HrAspire.Web.Client.Services.Account;

using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components.WebAssembly.Http;

public class CookieHttpMessageHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

        return base.SendAsync(request, cancellationToken);
    }
}
