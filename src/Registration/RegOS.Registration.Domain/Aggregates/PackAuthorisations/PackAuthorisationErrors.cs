namespace RegOS.Registration.Domain.Aggregates.PackAuthorisations;

public static class PackAuthorisationErrors
{
    public const string NotFound =
        "That pack authorisation does not exist.";

    public const string TenantRequired =
        "A pack authorisation must belong to a tenant.";

    public const string PackRequired =
        "Name the pack this licence authorises.";

    /// <remarks>
    /// The fact a foreign key could not carry: a licence granted in 2021 that
    /// gained its 100-pack in 2024 has two dates, and only one of them is the
    /// registration's.
    /// </remarks>
    public const string AuthorisedOnRequired =
        "Say when this pack became authorised — it is often later than the "
        + "licence itself.";

    public const string AlreadyAuthorised =
        "This licence already authorises that pack.";

    public const string PackBelongsToAnotherMarket =
        "That pack is sold in a different market from this licence.";

    public const string RegistrationDoesNotExist =
        "That registration does not exist.";
}
