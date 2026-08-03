using RegOS.ReferenceData.Domain.Geography.Country;
using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Product.Domain.Product;
using RegOS.ReferenceData.Domain.ApplicationType;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

using ApplicationTypeEntity = RegOS.ReferenceData.Domain.ApplicationType.ApplicationType;

namespace RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;

public sealed class RegulatoryApplication
{
    public const string TenantRequired = "Tenant is required.";
    public const string ProductRequired = "Product is required.";
    public const string CountryRequired = "Country is required.";
    public const string AuthorityRequired = "Authority is required.";
    public const string ApplicationTypeRequired = "Application type is required.";
    public const string ApplicationTypeAuthorityMismatch =
        "Application type does not belong to the application's authority.";
    public const string ApplicantOrganizationRequired = "Applicant organization is required.";

    public const int ApplicationNumberMaxLength = 100;

    public const string ApplicationNumberRequired =
        "An application number is what the authority assigned. Enter it as they "
        + "issued it.";

    public const string ApplicationNumberTooLong =
        "That application number is too long.";
    public const string NameRequired = "Name is required.";

    private RegulatoryApplication(
        RegulatoryApplicationId id,
        TenantId tenantId,
        GlobalProductId globalProductId,
        CountryId countryId,
        AuthorityId authorityId,
        ApplicationTypeId applicationTypeId,
        OrganizationId applicantOrganizationId,
        string name,
        DateTime createdOn)
    {
        Id = id;
        TenantId = tenantId;
        GlobalProductId = globalProductId;
        CountryId = countryId;
        AuthorityId = authorityId;
        ApplicationTypeId = applicationTypeId;
        ApplicantOrganizationId = applicantOrganizationId;
        Name = name;
        Status = ApplicationStatus.Draft;
        CreatedOn = createdOn;
    }

    public RegulatoryApplicationId Id { get; }

    /// <summary>
    /// The owning tenant — who this record belongs to. Not the same concept as
    /// <see cref="ApplicantOrganizationId"/>, which is who the application is
    /// filed on behalf of: a tenant can file for a partner, so the two may
    /// legitimately differ (ADR-030).
    /// </summary>
    public TenantId TenantId { get; }

    public GlobalProductId GlobalProductId { get; }

    public CountryId CountryId { get; private set; }

    public AuthorityId AuthorityId { get; private set; }

    /// <summary>
    /// What kind of application this is — IND, NDA, 510(k). eCTD's
    /// <c>application-type</c>.
    /// </summary>
    /// <remarks>
    /// This lived on <c>Submission</c> under the name <c>SubmissionType</c>
    /// until EPIC-007a S001 (evidence E11). It is invariant across every
    /// submission in an application — one application is one
    /// <see cref="ApplicationNumber"/>, and every sequence filed under an IND
    /// is an IND — which is what places it on the aggregate root.
    /// </remarks>
    public ApplicationTypeId ApplicationTypeId { get; private set; }

    public OrganizationId ApplicantOrganizationId { get; private set; }

    public string Name { get; private set; }

    public string? ApplicationNumber { get; private set; }

    public ApplicationStatus Status { get; private set; }

    public DateTime CreatedOn { get; }

    /// <param name="applicationType">
    /// The reference-data entity, not its id, because the factory enforces a
    /// rule about it: an application's type must belong to the authority the
    /// application is filed with. Passing the id alone would move that check
    /// back out to a handler, which is where it lived — on the wrong aggregate,
    /// and fired one step too late — before EPIC-007a S001.
    /// </param>
    public static RegulatoryApplication Create(
        TenantId tenantId,
        GlobalProductId globalProductId,
        CountryId countryId,
        AuthorityId authorityId,
        ApplicationTypeEntity applicationType,
        OrganizationId applicantOrganizationId,
        string name)
    {
        if (tenantId is null)
            throw new DomainException(TenantRequired);

        if (globalProductId == default)
            throw new DomainException(ProductRequired);

        if (countryId == default)
            throw new DomainException(CountryRequired);

        if (authorityId == default)
            throw new DomainException(AuthorityRequired);

        if (applicationType is null)
            throw new DomainException(ApplicationTypeRequired);

        // An IND is an FDA thing; an ARTG inclusion is a TGA thing. Filing a
        // TGA application type with the FDA is not a data-entry slip to be
        // caught downstream — it is not an application.
        if (applicationType.AuthorityId != authorityId)
            throw new DomainException(ApplicationTypeAuthorityMismatch);

        if (applicantOrganizationId == default)
            throw new DomainException(ApplicantOrganizationRequired);

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(NameRequired);

        return new RegulatoryApplication(
            RegulatoryApplicationId.New(),
            tenantId,
            globalProductId,
            countryId,
            authorityId,
            applicationType.Id,
            applicantOrganizationId,
            name.Trim(),
            DateTime.UtcNow);
    }

    /// <summary>
    /// The number the authority assigned to this application.
    /// </summary>
    /// <remarks>
    /// <b>Stored exactly as assigned, and constrained only by being said.</b>
    /// An application number is not RegOS's to invent or to shape: FDA issues
    /// six digits, and Health Canada, the EMA and the TGA each issue something
    /// else. A format rule here would make one authority's convention every
    /// authority's law — so the shape is checked at the boundary that cares,
    /// which is the FDA renderer ([ADR-055](../../../../../docs/adr/ADR-055-when-an-authority-required-fact-becomes-a-domain-fact.md)).
    /// <para>
    /// <b>Recording it is not part of <c>Create</c></b>, because an application
    /// exists in RegOS before the authority has answered. That is the ordinary
    /// case, not an edge one — the number arrives with the acknowledgement.
    /// </para>
    /// <para>
    /// <b>Correctable here, frozen elsewhere.</b> The aggregate permits a
    /// correction because RegOS's record of an external fact can simply be
    /// wrong, and refusing would force someone to delete a regulatory record to
    /// fix a typo. What it must not do is change after a sequence has carried it
    /// to the authority — and that is a fact about *submissions*, which this
    /// aggregate cannot see, so the handler enforces it. The same division S003
    /// drew for the two rules that could not live among its invariants.
    /// </para>
    /// </remarks>
    public void RecordApplicationNumber(string applicationNumber)
    {
        if (string.IsNullOrWhiteSpace(applicationNumber))
            throw new DomainException(ApplicationNumberRequired);

        var trimmed = applicationNumber.Trim();

        if (trimmed.Length > ApplicationNumberMaxLength)
            throw new DomainException(ApplicationNumberTooLong);

        ApplicationNumber = trimmed;
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(NameRequired);

        Name = name.Trim();
    }

    /// <summary>Draft or OnHold -> Active.</summary>
    public void Activate()
    {
        if (Status == ApplicationStatus.Closed)
            throw new BusinessRuleViolationException(
                ApplicationErrors.ApplicationAlreadyClosed);

        if (Status == ApplicationStatus.Active)
            throw new BusinessRuleViolationException(
                ApplicationErrors.ApplicationAlreadyActive);

        // Status is Draft or OnHold.
        Status = ApplicationStatus.Active;
    }

    /// <summary>Active -> OnHold.</summary>
    public void PutOnHold()
    {
        if (Status == ApplicationStatus.Closed)
            throw new BusinessRuleViolationException(
                ApplicationErrors.ApplicationAlreadyClosed);

        if (Status != ApplicationStatus.Active)
            throw new BusinessRuleViolationException(
                ApplicationErrors.InvalidStatusTransition);

        Status = ApplicationStatus.OnHold;
    }

    /// <summary>Active -> Closed. Closed is terminal.</summary>
    public void Close()
    {
        if (Status == ApplicationStatus.Closed)
            throw new BusinessRuleViolationException(
                ApplicationErrors.ApplicationAlreadyClosed);

        if (Status != ApplicationStatus.Active)
            throw new BusinessRuleViolationException(
                ApplicationErrors.InvalidStatusTransition);

        Status = ApplicationStatus.Closed;
    }
}
