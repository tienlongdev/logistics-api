namespace Logistics.Api.BuildingBlocks.Application.Results;

/// <summary>
/// Result generic.
/// </summary>
public sealed class Result<T> : Result
{
    private readonly T? _value;

    private Result(bool isSuccess, Error error, T? value)
        : base(isSuccess, error)
    {
        _value = value;
    }

    public T Value
        => IsSuccess
            ? _value!
            : throw new InvalidOperationException("Không thể truy cập Value khi Result là Failure.");

    public static Result<T> Success(T value) => new(true, Error.None, value);

    public static new Result<T> Failure(Error error) => new(false, error, default);
}
