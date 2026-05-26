using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        string clientMessage;
        switch (exception)
        {
            case InvalidIdException notFoundEx:
                _logger.LogWarning("Resource not found: {Message}", notFoundEx.Message);
                httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                httpContext.Response.ContentType = "text/plain";
                await httpContext.Response.WriteAsync(notFoundEx.Message, cancellationToken);
                return true;

            case DomainException domainEx:
                _logger.LogWarning("Domain rule violation: {Message}", domainEx.Message);
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                clientMessage = domainEx.Message; 
                break;

            case ApplicationException appEx:
                _logger.LogWarning("Application service error: {Message}", appEx.Message);
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                clientMessage = appEx.Message; 
                break;

            default:
                _logger.LogError(exception, "An unhandled system infrastructure error occurred.");

                httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                clientMessage = "Something went wrong on our end. Please try again later.";
                break;
        }
        httpContext.Response.ContentType = "text/plain";
        await httpContext.Response.WriteAsync(clientMessage, cancellationToken);

        return true;
    }
}