using RegOS.MasterData.Domain.Geography.Country;
using RegOS.MasterData.Domain.Regulatory.Authority;
using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Product.Domain.Product;

namespace RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;

public sealed class RegulatoryApplication
{
    public const string ProductRequired = "Product is required.";
    public const string CountryRequired = "Country is required.";
    public const string AuthorityRequired = "Authority is required.";
    public const string ApplicantOrganizationRequired = "Applicant organization is required.";
    public const string NameRequired = "Name is required.";

    private RegulatoryApplication(
        RegulatoryApplicationId id,
        ProductId productId,
        CountryId countryId,
        AuthorityId authorityId,
        OrganizationId applicantOrganizationId,
        string name,
        DateTime createdOn)
    {
        Id = id;
        ProductId = productId;
        CountryId = countryId;
        AuthorityId = authorityId;
        ApplicantOrganizationId = applicantOrganizationId;
        Name = name;
        Status = RegulatoryApplicationStatus.Draft;
        CreatedOn = createdOn;
    }

    public RegulatoryApplicationId Id { get; }

    public ProductId ProductId { get; }

    public CountryId CountryId { get; private set; }

    public AuthorityId AuthorityId { get; private set; }

    public OrganizationId ApplicantOrganizationId { get; private set; }

    public string Name { get; private set; }

    public string? ApplicationNumber { get; private set; }

    public RegulatoryApplicationStatus Status { get; private set; }

    public DateTime CreatedOn { get; }

    public static RegulatoryApplication Create(
        ProductId productId,
        CountryId countryId,
        AuthorityId authorityId,
        OrganizationId applicantOrganizationId,
        string name)
    {
        if (productId == default)
            throw new ArgumentException(ProductRequired, nameof(productId));

        if (countryId == default)
            throw new ArgumentException(CountryRequired, nameof(countryId));

        if (authorityId == default)
            throw new ArgumentException(AuthorityRequired, nameof(authorityId));

        if (applicantOrganizationId == default)
            throw new ArgumentException(ApplicantOrganizationRequired, nameof(applicantOrganizationId));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(NameRequired, nameof(name));

        return new RegulatoryApplication(
            RegulatoryApplicationId.New(),
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
            throw new ArgumentException(NameRequired, nameof(name));

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
