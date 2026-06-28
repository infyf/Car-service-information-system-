namespace AutoServ.Core.Models
{
    public class Result<T>
    {
        public bool IsSuccess { get; private set; }
        public string? Error { get; private set; }
        public T? Value { get; private set; }

        private Result(bool isSuccess, string? error, T? value)
        {
            IsSuccess = isSuccess;
            Error = error;
            Value = value;
        }

        public static Result<T> Success(T value) => new Result<T>(true, null, value);
        public static Result<T> Failure(string error) => new Result<T>(false, error, default);
    }

    public class Result
    {
        public bool IsSuccess { get; private set; }
        public string? Error { get; private set; }

        private Result(bool isSuccess, string? error)
        {
            IsSuccess = isSuccess;
            Error = error;
        }

        public static Result Success() => new Result(true, null);
        public static Result Failure(string error) => new Result(false, error);
    }
}