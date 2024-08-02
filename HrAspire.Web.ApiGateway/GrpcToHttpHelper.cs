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
            var httpStatusCode = re.StatusCode switch
            {
                StatusCode.Unauthenticated => HttpStatusCode.Unauthorized,
                StatusCode.PermissionDenied => HttpStatusCode.Forbidden,
                StatusCode.NotFound => HttpStatusCode.NotFound,
                StatusCode.AlreadyExists or StatusCode.Aborted => HttpStatusCode.Conflict,
                StatusCode.ResourceExhausted => HttpStatusCode.TooManyRequests,
                StatusCode.DeadlineExceeded => HttpStatusCode.GatewayTimeout,
                StatusCode.Unimplemented => HttpStatusCode.NotImplemented,
                StatusCode.Unavailable => HttpStatusCode.ServiceUnavailable,
                StatusCode.Cancelled => (HttpStatusCode)499,
                StatusCode.OK => HttpStatusCode.OK, // Should never happen
                _ => HttpStatusCode.InternalServerError,
            };

            return Results.StatusCode((int)httpStatusCode);
        }
    }
}
