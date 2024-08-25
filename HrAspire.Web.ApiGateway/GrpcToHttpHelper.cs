namespace HrAspire.Web.ApiGateway;

using System.Net;

using Grpc.Core;

using Microsoft.AspNetCore.Http;

public static class GrpcToHttpHelper
{
    public static async Task<IResult> HandleGrpcCallAsync(Func<Task<IResult>> handleGrpcCallFunc)
    {
        ArgumentNullException.ThrowIfNull(handleGrpcCallFunc);

        try
        {
            return await handleGrpcCallFunc();
        }
        catch (RpcException re) when (re.StatusCode is StatusCode.InvalidArgument or StatusCode.OutOfRange or StatusCode.FailedPrecondition)
        {
            return Results.Problem(re.Status.Detail, statusCode: (int)HttpStatusCode.BadRequest);
        }
        catch (RpcException re)
        {
            var httpStatusCode = GrpcToHttpStatusCode(re.StatusCode);
            return Results.StatusCode((int)httpStatusCode);
        }
    }

    public static async Task HandleGrpcCallAsync(Func<Task> handleGrpcCallFunc, HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(handleGrpcCallFunc);
        ArgumentNullException.ThrowIfNull(httpContext);

        try
        {
            await handleGrpcCallFunc();
        }
        catch (RpcException re) when (re.StatusCode is StatusCode.InvalidArgument or StatusCode.OutOfRange or StatusCode.FailedPrecondition)
        {
            await Results.Problem(re.Status.Detail, statusCode: (int)HttpStatusCode.BadRequest).ExecuteAsync(httpContext);
        }
        catch (RpcException re)
        {
            var httpStatusCode = GrpcToHttpStatusCode(re.StatusCode);
            await Results.StatusCode((int)httpStatusCode).ExecuteAsync(httpContext);
        }
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
