namespace RegOS.Api.Tenancy;

/// <summary>
/// Messages for a request whose tenant cannot be determined. All map to 400:
/// the request is malformed, not in conflict with any business state.
/// </summary>
public static class TenantErrors
{
    public const string Missing =
        $"The '{HeaderTenantContext.HeaderName}' header is required.";

    public const string Malformed =
        $"The '{HeaderTenantContext.HeaderName}' header must be a non-empty GUID.";

    public const string Unavailable =
        "The tenant cannot be resolved outside of an HTTP request.";
}
