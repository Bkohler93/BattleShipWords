namespace BattleshipWithWords.Utilities;

public class Result {
    public bool Success { get; }
    public string Error { get; }

    protected Result(bool success, string error = null) {
        Success = success;
        Error = error;
    }

    public static Result Ok() => new Result(true);
    public static Result Fail(string error) => new Result(false, error);
}

public class Result<T> : Result {
    public T Value { get; }

    private Result(T value) : base(true) {
        Value = value;
    }

    private Result(string error) : base(false, error) {}

    public static Result<T> Ok(T value) => new Result<T>(value);
    public new static Result<T> Fail(string error) => new Result<T>(error);
}