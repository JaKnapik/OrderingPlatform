using Microsoft.AspNetCore.Diagnostics;
using Ordering.API.Common;

namespace Ordering.API.Infrastructure.Middleware;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
	public async ValueTask<bool> TryHandleAsync(
		HttpContext httpContext,
		Exception exception,
		CancellationToken cancellationToken)
	{
		logger.LogError(exception, "An unhandled exception occured: {Message}", exception.Message);

		var response = Result.Failure<object>(
			[exception.Message],
			"A server error occured.");

		httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
		await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);
		return true;
	}
}
