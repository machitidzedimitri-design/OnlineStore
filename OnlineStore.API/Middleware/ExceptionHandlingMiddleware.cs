using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OnlineStore.Core.Exceptions;

namespace OnlineStore.API.Middleware;

/// <summary>
/// Central error handling: services throw typed exceptions, this middleware converts them
/// to consistent ProblemDetails responses. No try/catch is needed in controllers.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            _logger.LogInformation("Request was cancelled by the client.");
        }
        catch (Exception ex)
        {
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception ex)
    {
        var (status, title, detail) = ex switch
        {
            NotFoundException e => (StatusCodes.Status404NotFound, "Not Found", e.Message),
            BadRequestException e => (StatusCodes.Status400BadRequest, "Bad Request", e.Message),
            ConflictException e => (StatusCodes.Status409Conflict, "Conflict", e.Message),
            ExternalServiceException e => (StatusCodes.Status502BadGateway, "Bad Gateway", e.Message),
            // Race condition safety net: unique index violation (e.g. duplicate email)
            DbUpdateException { InnerException: SqlException { Number: 2601 or 2627 } } =>
                (StatusCodes.Status409Conflict, "Conflict", "A record with the same unique value already exists."),
            _ => (StatusCodes.Status500InternalServerError, "Server Error", "An unexpected error occurred.")
        };

        if (status >= 500) _logger.LogError(ex, "Unhandled exception");
        else _logger.LogWarning("Handled {Type}: {Message}", ex.GetType().Name, ex.Message);

        if (context.Response.HasStarted) return;

        context.Response.Clear();
        context.Response.StatusCode = status;

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        await context.Response.WriteAsJsonAsync(problem, (JsonSerializerOptions?)null, "application/problem+json");
    }
}
