using RegOS.Platform.Application.Exceptions;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Api.Middleware;

/// <summary>
/// Translates domain and application rule violations into HTTP responses so the
/// API is properly RESTful instead of surfacing everything as 500:
/// <list type="bullet">
///   <item><see cref="DomainException"/> (a broken domain invariant) -> 400.</item>
///   <item><see cref="BusinessRuleViolationException"/> (an application rule such
///   as an inactive organization or duplicate email) -> 409.</item>
///   <item><see cref="NotFoundException"/> (missing record, or one outside the
///   caller's organization) -> 404.</item>
/// </list>
/// Anything else is left to propagate to the default handler (500), preserving
/// today's behaviour for the modules that have not yet adopted this mapping.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (NotFoundException exception)
        {
            await WriteProblemAsync(
                context, StatusCodes.Status404NotFound, exception.Message);
        }
        catch (BusinessRuleViolationException exception)
        {
            await WriteProblemAsync(
                context, StatusCodes.Status409Conflict, exception.Message);
        }
        catch (DomainException exception)
        {
            await WriteProblemAsync(
                context, StatusCodes.Status400BadRequest, exception.Message);
        }
    }

    private static Task WriteProblemAsync(
        HttpContext context,
        int statusCode,
        string detail)
        => Results.Problem(detail: detail, statusCode: statusCode)
            .ExecuteAsync(context);
}
