using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SmartBuilding.Application.Common;

namespace SmartBuilding.Api.ErrorHandling;

internal sealed class ApiExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        int statusCode = exception is ApplicationValidationException
            ? StatusCodes.Status400BadRequest
            : StatusCodes.Status500InternalServerError;
        string title = statusCode == StatusCodes.Status400BadRequest
            ? "Invalid request."
            : "An unexpected error occurred.";

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled API exception.");
        }
        else
        {
            logger.LogWarning(exception, "API request validation failed.");
        }

        httpContext.Response.StatusCode = statusCode;
        ProblemDetails problemDetails = exception is ApplicationValidationException validationException
            ? new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                [validationException.PropertyName] = [validationException.Message]
            })
            {
                Title = title,
                Status = statusCode
            }
            : new ProblemDetails
            {
                Title = title,
                Status = statusCode
            };

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problemDetails
        });
    }
}