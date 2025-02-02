namespace HrAspire.Web.ApiGateway;

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using Grpc.Core;

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

public class GrpcExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GrpcExceptionHandler> logger;

    public GrpcExceptionHandler(ILogger<GrpcExceptionHandler> logger)
    {
        this.logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not RpcException grpcException)
        {
            return false;
        }

        var grpcStatus = grpcException.Status;
        if (grpcStatus.StatusCode is StatusCode.InvalidArgument or StatusCode.OutOfRange or StatusCode.FailedPrecondition)
        {
            await Results.Problem(grpcStatus.Detail, statusCode: (int)HttpStatusCode.BadRequest).ExecuteAsync(httpContext);
        }
        else
        {
            var httpStatusCode = GrpcToHttpStatusCode(grpcStatus.StatusCode);

            // Don't log trivial 404 errors
            if (httpStatusCode != HttpStatusCode.NotFound)
            {
                this.logger.LogError("Exception occurred: {Exception}", exception.ToString());
            }

            await Results.StatusCode((int)httpStatusCode).ExecuteAsync(httpContext);
        }

        return true;
    }

    private static HttpStatusCode GrpcToHttpStatusCode(StatusCode grpcStatusCode)
        => grpcStatusCode switch
        {
            StatusCode.OK => HttpStatusCode.OK,
            StatusCode.InvalidArgument or StatusCode.OutOfRange or StatusCode.FailedPrecondition => HttpStatusCode.BadRequest,
            StatusCode.Unauthenticated => HttpStatusCode.Unauthorized,
            StatusCode.PermissionDenied => HttpStatusCode.Forbidden,
            StatusCode.NotFound => HttpStatusCode.NotFound,
            StatusCode.AlreadyExists or StatusCode.Aborted => HttpStatusCode.Conflict,
            StatusCode.ResourceExhausted => HttpStatusCode.TooManyRequests,
            StatusCode.DeadlineExceeded => HttpStatusCode.GatewayTimeout,
            StatusCode.Unimplemented => HttpStatusCode.NotImplemented,
            StatusCode.Unavailable => HttpStatusCode.ServiceUnavailable,
            StatusCode.Cancelled => (HttpStatusCode)499,
            _ => HttpStatusCode.InternalServerError,
        };
}
