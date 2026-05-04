namespace MODELS.ProcureAccess.Entities;

public class Result
{
    public bool Succeeded { get; set; }
    public string? Error { get; set; }

    public static Result Success() => new() { Succeeded = true };
    public static Result Failure(string error) => new() { Succeeded = false, Error = error };
}

public class Result<T> : Result
{
    public T? Value { get; set; }

    public static Result<T> Success(T value) =>
        new() { Succeeded = true, Value = value };

    public static new Result<T> Failure(string error) =>
        new() { Succeeded = false, Error = error };
}
