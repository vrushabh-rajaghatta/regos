namespace RegOS.Api.Authentication;

public static class CurrentUserErrors
{
    /// <summary>
    /// One message for every way identity can fail to resolve — absent token,
    /// missing claim, unparseable claim. The same reasoning as sign-in
    /// (ADR-022): the caller learns that they are not authenticated, not which
    /// part of their token disappointed us.
    /// </summary>
    public const string NotAuthenticated =
        "Authentication is required.";
}
