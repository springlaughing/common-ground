namespace CommonGround.SharedKernel.Domain;

public sealed class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? ErrorCode { get; }
    public string? ErrorMessage { get; }

    internal Result(T value)
    {
        IsSuccess = true;
        Value = value;
    }

    internal Result(string errorCode, string errorMessage)
    {
        IsSuccess = false;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }
}

public static class Result
{
    public static Result<T> Success<T>(T value) => new(value);
    public static Result<T> Failure<T>(string errorCode, string errorMessage) => new(errorCode, errorMessage);
}
