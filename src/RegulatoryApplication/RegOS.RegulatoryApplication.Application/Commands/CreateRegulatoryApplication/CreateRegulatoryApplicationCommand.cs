using RegOS.ReferenceData.Domain.ApplicationType;
using RegOS.ReferenceData.Domain.Geography.Country;
using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Product.Domain.Product;

namespace RegOS.RegulatoryApplication.Application.Commands.CreateRegulatoryApplication;

public sealed record CreateRegulatoryApplicationCommand(
    GlobalProductId GlobalProductId,
    CountryId CountryId,
    AuthorityId AuthorityId,
    ApplicationTypeId ApplicationTypeId,
    OrganizationId ApplicantOrganizationId,
    string Name);
