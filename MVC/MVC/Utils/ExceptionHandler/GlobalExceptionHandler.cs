using Microsoft.AspNetCore.Diagnostics;

namespace API.Utils.ExceptionHandler;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "An unhandled exception occurred.");

        var response = httpContext.Response;
        response.ContentType = "application/json";

        var statusCode = exception is UnauthorizedAccessException
            ? StatusCodes.Status401Unauthorized
            : StatusCodes.Status500InternalServerError;

        response.StatusCode = statusCode;

        var message = statusCode == StatusCodes.Status401Unauthorized
            ? "Unauthorized. Please sign in."
            : "An unexpected error occurred.";

        await response.WriteAsJsonAsync(new
        {
            error = message
        }, cancellationToken);

        return true;
    }
}
