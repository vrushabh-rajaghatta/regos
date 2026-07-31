namespace RegOS.Registration.Application;

/// <summary>
/// Cross-aggregate rule messages: they coordinate a Product with reference data
/// and an Organization, so they live in the application layer rather than the
/// aggregate.
/// </summary>
public static class RegistrationRuleErrors
{
    public const string MedicinalProductDoesNotExist =
        "Medicinal product does not exist.";

    public const string AuthorityDoesNotExist =
        "Authority does not exist.";

    public const string AuthorityNotInCountry =
        "Authority does not belong to the selected country.";

    public const string OrganizationDoesNotExist =
        "Holder organization does not exist.";

    public const string OrganizationInactive =
        "The holder organization is not active.";

    public const string ApplicationDoesNotExist =
        "The originating application does not exist.";

    public const string ApplicationNotForProduct =
        "The originating application belongs to a different product.";

    public const string RegistrationDoesNotExist =
        "Registration does not exist.";
}
