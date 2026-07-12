using RegOS.Product.Domain.Product;

namespace RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;

public sealed class RegulatoryApplication
{
    private RegulatoryApplication(
        RegulatoryApplicationId id,
        ProductId productId,
        Guid authorityId,
        Guid countryId,
        Guid applicantOrganizationId,
        string name)
    {
        Id = id;
        ProductId = productId;
        AuthorityId = authorityId;
        CountryId = countryId;
        ApplicantOrganizationId = applicantOrganizationId;
        Name = name;
        Status = RegulatoryApplicationStatus.Draft;
    }

    public RegulatoryApplicationId Id { get; }

    public ProductId ProductId { get; }

    public Guid AuthorityId { get; private set; }

    public Guid CountryId { get; private set; }

    public Guid ApplicantOrganizationId { get; private set; }

    public string Name { get; private set; }

    public string? ApplicationNumber { get; private set; }

    public RegulatoryApplicationStatus Status { get; private set; }

    public static RegulatoryApplication Create(
        ProductId productId,
        Guid authorityId,
        Guid countryId,
        Guid applicantOrganizationId,
        string name)
    {
        if (productId == default)
            throw new ArgumentException("Product is required.", nameof(productId));

        if (authorityId == Guid.Empty)
            throw new ArgumentException("Authority is required.", nameof(authorityId));

        if (countryId == Guid.Empty)
            throw new ArgumentException("Country is required.", nameof(countryId));

        if (applicantOrganizationId == Guid.Empty)
            throw new ArgumentException("Applicant organization is required.", nameof(applicantOrganizationId));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));

        return new RegulatoryApplication(
            RegulatoryApplicationId.New(),
            productId,
            authorityId,
            countryId,
            applicantOrganizationId,
            name.Trim());
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));

        Name = name.Trim();
    }

    public void Activate()
    {
        Status = RegulatoryApplicationStatus.Active;
    }

    public void Approve()
    {
        Status = RegulatoryApplicationStatus.Approved;
    }

    public void Reject()
    {
        Status = RegulatoryApplicationStatus.Rejected;
    }

    public void Withdraw()
    {
        Status = RegulatoryApplicationStatus.Withdrawn;
    }

    public void Archive()
    {
        Status = RegulatoryApplicationStatus.Archived;
    }
}
