namespace RegOS.Registration.Domain.Aggregates.Registration;

public static class RegistrationErrors
{
    public const string TenantRequired =
        "Tenant is required.";

    public const string ProductRequired =
        "Product is required.";

    public const string CountryRequired =
        "Country is required.";

    public const string AuthorityRequired =
        "Authority is required.";

    public const string HolderOrganizationRequired =
        "Holder organization is required.";

    public const string OccurredOnRequired =
        "The date the status took effect is required.";

    // Approval
    public const string RegistrationNumberRequired =
        "A registration number is required to record an approval.";

    public const string RegistrationNumberTooLong =
        "The registration number is too long.";

    public const string ApprovalAlreadyRecorded =
        "This registration is already approved.";

    public const string ExpiryBeforeApproval =
        "A registration cannot expire before it was approved.";

    public const string NoteTooLong =
        "The note is too long.";
}
