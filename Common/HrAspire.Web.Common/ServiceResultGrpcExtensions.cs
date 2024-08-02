namespace HrAspire.Web.Common;

using Grpc.Core;

using HrAspire.Business.Common;

public static class ServiceResultGrpcExtensions
{
    public static RpcException ToRpcException(this ServiceResult serviceResult)
    {
        ArgumentNullException.ThrowIfNull(serviceResult);

        if (!serviceResult.IsError)
        {
            throw new InvalidOperationException("Cannot build an RPC exception from a successful service result.");
        }

        if (serviceResult.ErrorMessage == ServiceResult.ErrorNotFound.ErrorMessage)
        {
            return new RpcException(new Status(StatusCode.NotFound, detail: string.Empty));
        }

        return new RpcException(new Status(StatusCode.InvalidArgument, serviceResult.ErrorMessage!));
    }
}
