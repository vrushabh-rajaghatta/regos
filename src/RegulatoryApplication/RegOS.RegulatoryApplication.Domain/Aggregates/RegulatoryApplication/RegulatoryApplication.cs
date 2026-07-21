using RegOS.ReferenceData.Domain.Geography.Country;
using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Product.Domain.Product;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;

public sealed class RegulatoryApplication
{
    public const string TenantRequired = "Tenant is required.";
    public const string ProductRequired = "Product is required.";
    public const string CountryRequired = "Country is required.";
    public const string AuthorityRequired = "Authority is required.";
    public const string ApplicantOrganizationRequired = "Applicant organization is required.";
    public const string NameRequired = "Name is required.";

    private RegulatoryApplication(
        RegulatoryApplicationId id,
        TenantId tenantId,
        ProductId productId,
        CountryId countryId,
        AuthorityId authorityId,
        OrganizationId applicantOrganizationId,
        string name,
        DateTime createdOn)
    {
        Id = id;
        TenantId = tenantId;
        ProductId = productId;
        CountryId = countryId;
        AuthorityId = authorityId;
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

    public ProductId ProductId { get; }

    public CountryId CountryId { get; private set; }

    public AuthorityId AuthorityId { get; private set; }

    public OrganizationId ApplicantOrganizationId { get; private set; }

    public string Name { get; private set; }

    public string? ApplicationNumber { get; private set; }

    public ApplicationStatus Status { get; private set; }

    public DateTime CreatedOn { get; }

    public static RegulatoryApplication Create(
        TenantId tenantId,
        ProductId productId,
        CountryId countryId,
        AuthorityId authorityId,
        OrganizationId applicantOrganizationId,
        string name)
    {
        if (tenantId is null)
            throw new DomainException(TenantRequired);

        if (productId == default)
            throw new DomainException(ProductRequired);

        if (countryId == default)
            throw new DomainException(CountryRequired);

        if (authorityId == default)
            throw new DomainException(AuthorityRequired);

        if (applicantOrganizationId == default)
            throw new DomainException(ApplicantOrganizationRequired);

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(NameRequired);

        return new RegulatoryApplication(
            RegulatoryApplicationId.New(),
            tenantId,
            productId,
            countryId,
            authorityId,
            applicantOrganizationId,
            name.Trim(),
            DateTime.UtcNow);
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
