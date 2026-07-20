using RegOS.SharedKernel.Exceptions;

namespace RegOS.Api.Middleware;

/// <summary>
/// The single place where RegOS's shared failure model becomes HTTP. Every
/// bounded context throws the same three types and no endpoint catches business
/// exceptions, so this mapping is the whole contract:
/// <list type="bullet">
///   <item><see cref="NotFoundException"/> (the resource does not exist, or is
///   invisible to this caller) -> 404.</item>
///   <item><see cref="BusinessRuleViolationException"/> (the request is valid
///   but current business state forbids it) -> 409.</item>
///   <item><see cref="DomainException"/> (the request itself is invalid) -> 400.</item>
/// </list>
/// Anything else propagates to the default handler as a 500, which is the
/// correct signal: it means something genuinely unexpected happened.
/// </summary>
/// <remarks>
/// Catch order is load-bearing. <see cref="NotFoundException"/> and
/// <see cref="BusinessRuleViolationException"/> both derive from
/// <see cref="DomainException"/>, so the most specific types must be caught
/// first or everything would collapse to 400.
/// </remarks>
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
