using RegOS.Product.Domain.Product;

namespace RegOS.RegulatoryApplication.Domain.Aggregates.Application;

public sealed class Application
{
    private Application(
        ApplicationId id,
        ProductId productId,
        Guid authorityId,
        Guid countryId,
        Guid applicantOrganizationId,
        string displayName)
    {
        Id = id;
        ProductId = productId;
        AuthorityId = authorityId;
        CountryId = countryId;
        ApplicantOrganizationId = applicantOrganizationId;
        DisplayName = displayName;
        Status = ApplicationStatus.Draft;
    }

    public ApplicationId Id { get; }

    public ProductId ProductId { get; }

    public Guid AuthorityId { get; private set; }

    public Guid CountryId { get; private set; }

    public Guid ApplicantOrganizationId { get; private set; }

    public string DisplayName { get; private set; }

    public string? ApplicationNumber { get; private set; }

    public ApplicationStatus Status { get; private set; }

    public static Application Register(
        ProductId productId,
        Guid authorityId,
        Guid countryId,
        Guid applicantOrganizationId,
        string displayName)
    {
        if (productId == default)
            throw new ArgumentException("Product is required.", nameof(productId));

        if (authorityId == Guid.Empty)
            throw new ArgumentException("Authority is required.", nameof(authorityId));

        if (countryId == Guid.Empty)
            throw new ArgumentException("Country is required.", nameof(countryId));

        if (applicantOrganizationId == Guid.Empty)
            throw new ArgumentException("Applicant organization is required.", nameof(applicantOrganizationId));

        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name is required.", nameof(displayName));

        return new Application(
            ApplicationId.New(),
            productId,
            authorityId,
            countryId,
            applicantOrganizationId,
            displayName.Trim());
    }

    public void AssignApplicationNumber(string applicationNumber)
    {
        if (string.IsNullOrWhiteSpace(applicationNumber))
            throw new ArgumentException("Application number is required.", nameof(applicationNumber));

        ApplicationNumber = applicationNumber.Trim();
    }

    public void Rename(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name is required.", nameof(displayName));

        DisplayName = displayName.Trim();
    }

    public void Activate()
    {
        Status = ApplicationStatus.Active;
    }

    public void Approve()
    {
        Status = ApplicationStatus.Approved;
    }

    public void Reject()
    {
        Status = ApplicationStatus.Rejected;
    }

    public void Withdraw()
    {
        Status = ApplicationStatus.Withdrawn;
    }

    public void Archive()
    {
        Status = ApplicationStatus.Archived;
    }
}