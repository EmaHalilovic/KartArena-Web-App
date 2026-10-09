using FluentValidation;
using KartArena.Application.Common.Exceptions;
using KartArena.Shared.Dtos;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace KartArena.Infrastructure.Common;

/// <summary>
/// Global exception handler for unhandled exceptions.
/// Keeps the same ErrorDto format as the previous middleware.
/// </summary>
public sealed class MarketExceptionHandler(
    ILogger<MarketExceptionHandler> logger,
    IHostEnvironment env
) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext ctx, Exception ex, CancellationToken ct)
    {
        // If the response has already started, let it bubble up
        if (ctx.Response.HasStarted)
        {
            logger.LogWarning(ex, "Response already started, letting the exception bubble.");
            return false;
        }

        var traceId = Activity.Current?.Id ?? ctx.TraceIdentifier;

        logger.LogError(ex,
            "Unhandled exception. Path: {Path}, Method: {Method}, TraceId: {TraceId}, User: {User}",
            ctx.Request.Path,
            ctx.Request.Method,
            traceId,
            ctx.User.Identity?.Name ?? "anonymous");


        ctx.Response.ContentType = "application/json";
        ctx.Response.StatusCode = ex switch
        {
            MarketNotFoundException => StatusCodes.Status404NotFound,
            MarketConflictException or MarketBusinessRuleException => StatusCodes.Status409Conflict,
            ValidationException => StatusCodes.Status400BadRequest,
            MarketExternalServiceException => StatusCodes.Status502BadGateway,
            MarketConfigurationException => StatusCodes.Status500InternalServerError,
            _ => StatusCodes.Status500InternalServerError

        };

        var error = BuildErrorDto(ex, env.IsDevelopment(), traceId);

        await ctx.Response.WriteAsJsonAsync(error, cancellationToken: ct);
        return true; // prevents rethrowing the exception
    }

    private static ErrorDto BuildErrorDto(Exception ex, bool isDev, string traceId)
    {
        string code = "internal.error";
        string message = "An error occurred. Please try again.";

        switch (ex)
        {
            case MarketNotFoundException:
                code = "entity.not_found";
                message = ex.Message;
                break;

            case MarketConflictException:
                code = "entity.conflict";
                message = ex.Message;
                break;

            case MarketBusinessRuleException businessException:
                code = businessException.Code;
                message = businessException.Message;
                break;

            case MarketExternalServiceException externalException:
                code = externalException.Code;
                message = externalException.Message;
                break;

            case MarketConfigurationException configurationException:
                code = configurationException.Code;
                message = configurationException.Message;
                break;

            case ValidationException validationException:
                code = "validation.error";
                message = "Validation failed: " +
                    string.Join(
                        "; ",
                        validationException.Errors.Select(error =>
                            $"{error.PropertyName}: {error.ErrorMessage}"));
                break;
        }

        return new ErrorDto
        {
            Code = code,
            Message = message,
            TraceId = traceId,
            Details = isDev ? ex.ToString() : null // stack trace only in Development environment
        };
    }
}
