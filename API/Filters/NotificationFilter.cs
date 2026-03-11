using API.Utils.Notification;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Filters;

public class NotificationFilter(NotificationContext notificationContext, ILogger<NotificationFilter> logger) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var resultContext = await next();

        if (notificationContext.HasNotifications && !resultContext.ExceptionHandled)
        {
            var notifications = notificationContext.Notifications;

            var errors = notificationContext.Notifications.Select(n => n.Message);

            var problemDetails = new ValidationProblemDetails
            {
                Title = "One or more validation errors occurred.",
                Status = (int)notifications.First().ErrorCode,
                Instance = context.HttpContext.Request.Path,
                Errors =
                {
                    ["DomainErrors"] = [.. errors]
                }
            };

            resultContext.Result = new ObjectResult(problemDetails);

            logger.LogWarning("Domain notifications occurred: {Errors}", errors);
            
            return;
        }
    }
}
