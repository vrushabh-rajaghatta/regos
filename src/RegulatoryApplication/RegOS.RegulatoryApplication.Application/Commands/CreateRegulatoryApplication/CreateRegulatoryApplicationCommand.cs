using RegOS.Product.Domain.Product;

namespace RegOS.RegulatoryApplication.Application.Commands.CreateRegulatoryApplication;

public sealed record CreateRegulatoryApplicationCommand(
    ProductId ProductId,
    Guid AuthorityId,
    Guid CountryId,
    Guid ApplicantOrganizationId,
    string Name);
