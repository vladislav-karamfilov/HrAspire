namespace HrAspire.Web.Client.Services;

using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components.WebAssembly.Http;

public class CookieHttpMessageHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
        request.Headers.Add("X-Requested-With", ["XMLHttpRequest"]);

        return base.SendAsync(request, cancellationToken);
    }
}
