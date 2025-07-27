using ErrorOr;

namespace VerticalSliceArchitecture.Application.Common.Models;

public class ApiResponse<T>
{
    public bool IsSuccess { get; private set; }
    public string? Message { get; private set; }
    public T? Data { get; private set; }
    public int StatusCode { get; private set; }

    private ApiResponse(T data, string? message, int statusCode)
    {
        IsSuccess = true;
        Data = data;
        Message = message;
        StatusCode = statusCode;
    }

    private ApiResponse(string message, int statusCode)
    {
        IsSuccess = false;
        Message = message;
        StatusCode = statusCode;
        Data = default;
    }

    public static ApiResponse<T> Success(T data, string message = "İşlem başarılı.", int statusCode = 200)
    {
        return new ApiResponse<T>(data, message, statusCode);
    }

    public static ApiResponse<T> Fail(string errorMessage, int statusCode = 400)
    {
        return new ApiResponse<T>(errorMessage, statusCode);
    }

    public static ApiResponse<T> From(ErrorOr<T> result)
    {
        if (result.IsError)
        {
            var firstError = result.FirstError;
            var message = string.Join(" | ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
            var statusCode = firstError.Type switch
            {
                ErrorType.Validation => 400,
                ErrorType.Conflict => 409,
                ErrorType.NotFound => 404,
                _ => 500,
            };

            return Fail(message, statusCode);
        }

        return Success(result.Value);
    }
}