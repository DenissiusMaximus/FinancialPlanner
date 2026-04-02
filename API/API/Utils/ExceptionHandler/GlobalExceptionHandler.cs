using Microsoft.AspNetCore.Diagnostics;

namespace API.Utils.ExceptionHandler;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "An unhandled exception occurred.");

        var response = httpContext.Response;
        response.ContentType = "application/json";
        response.StatusCode = StatusCodes.Status500InternalServerError;

        await response.WriteAsJsonAsync(new
        {
            error = "An unexpected error occurred."
        }, cancellationToken);

        return true;
    }
}
