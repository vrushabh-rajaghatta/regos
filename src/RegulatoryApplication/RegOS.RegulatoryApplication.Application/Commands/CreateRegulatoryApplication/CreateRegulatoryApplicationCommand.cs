using RegOS.ReferenceData.Domain.Geography.Country;
using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Product.Domain.Product;

namespace RegOS.RegulatoryApplication.Application.Commands.CreateRegulatoryApplication;

public sealed record CreateRegulatoryApplicationCommand(
    ProductId ProductId,
    CountryId CountryId,
    AuthorityId AuthorityId,
    OrganizationId ApplicantOrganizationId,
    string Name);
