namespace Ordering.API.Common;


public record ApiResponse<T>(
	T? Data,
	bool IsSuccess,
	string[]? Errors = null,
	string? Message = null
	);
public static class Result
{
	public static ApiResponse<T> Success<T>(T data, string? message = null)
		=> new(data, true, null, message);
	public static ApiResponse<T> Failure<T>(string[] errors, string? message = null)
		=> new(default, false, errors, message);
	public static ApiResponse<T> Failure<T>(string error, string? message = null)
		=> new(default, false, [error], message);
}
