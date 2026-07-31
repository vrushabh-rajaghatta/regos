using RegOS.ReferenceData.Domain.Organization;

namespace RegOS.Persistence.Initialization.ReferenceData.Organization;

internal static class ContactRoleIds
{
    // Contact roles (8100...)
    public static readonly Guid QualifiedPerson =
        Guid.Parse("81000000-0000-0000-0000-000000000001");
    public static readonly Guid AuthorisedRepresentative =
        Guid.Parse("81000000-0000-0000-0000-000000000002");
    public static readonly Guid RegulatoryContact =
        Guid.Parse("81000000-0000-0000-0000-000000000003");
    public static readonly Guid ManufacturingContact =
        Guid.Parse("81000000-0000-0000-0000-000000000004");
    public static readonly Guid PharmacovigilanceContact =
        Guid.Parse("81000000-0000-0000-0000-000000000005");
    public static readonly Guid AuthorityReviewer =
        Guid.Parse("81000000-0000-0000-0000-000000000006");
}

/// <summary>
/// The baseline roles RegOS ships — all with a null tenant, so every tenant
/// sees them.
/// </summary>
/// <remarks>
/// Deliberately only the roles that mean something outside one company. A
/// Qualified Person is defined by EU legislation; "APAC Regulatory Lead" is a
/// company's own word, and that is what the tenant extension is for.
/// </remarks>
internal static class ContactRoles
{
    public static readonly IReadOnlyList<ContactRole> Data =
    [
        ContactRole.Create(
            new ContactRoleId(ContactRoleIds.QualifiedPerson),
            "QP",
            "Qualified Person",
            "Certifies batch release under EU GMP. A legislated role."),

        ContactRole.Create(
            new ContactRoleId(ContactRoleIds.AuthorisedRepresentative),
            "AR",
            "Authorised Representative",
            "Acts for a manufacturer established outside the market."),

        ContactRole.Create(
            new ContactRoleId(ContactRoleIds.RegulatoryContact),
            "REG",
            "Regulatory Contact",
            "The named point of contact for regulatory correspondence."),

        ContactRole.Create(
            new ContactRoleId(ContactRoleIds.ManufacturingContact),
            "MFG",
            "Manufacturing Contact",
            "The named point of contact at a manufacturing site."),

        ContactRole.Create(
            new ContactRoleId(ContactRoleIds.PharmacovigilanceContact),
            "PV",
            "Pharmacovigilance Contact",
            "Receives safety correspondence. Often the QPPV or their deputy."),

        ContactRole.Create(
            new ContactRoleId(ContactRoleIds.AuthorityReviewer),
            "HA-REVIEWER",
            "Health Authority Reviewer",
            "A named reviewer at the authority assessing a submission."),
    ];
}
