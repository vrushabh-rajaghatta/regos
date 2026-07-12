using RegOS.Product.Domain.Product;

namespace RegOS.RegulatoryApplication.Application.Commands.RegisterApplication;

public sealed record RegisterApplicationCommand(
    ProductId ProductId,
    Guid AuthorityId,
    Guid CountryId,
    Guid ApplicantOrganizationId,
    string DisplayName);