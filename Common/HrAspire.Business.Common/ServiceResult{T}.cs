namespace HrAspire.Business.Common;

using System;

public class ServiceResult<T> : ServiceResult
{
    public static readonly ServiceResult<T> EmptySuccess = new();
    public new static readonly ServiceResult<T> ErrorNotFound = new() { ErrorMessage = ErrorNotFoundMessage };

    protected ServiceResult()
    {
    }

    public T? Data { get; protected set; }

    public new static ServiceResult<T> Success(T? data) => new() { Data = data };

    public new static ServiceResult<T> Error(string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
        {
            throw new ArgumentException("Error service result cannot have null or white space error message.");
        }

        return new ServiceResult<T> { ErrorMessage = errorMessage };
    }
}
