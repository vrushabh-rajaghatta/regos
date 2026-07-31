using RegOS.Product.Application.Services;
using RegOS.Product.Domain.Product;
using RegOS.SharedKernel.Abstractions;

namespace RegOS.Product.Application.Commands.CreateMedicinalProduct;

public sealed class CreateMedicinalProductHandler
{
    private readonly IMedicinalProductPolicy _policy;
    private readonly IMedicinalProductRepository _repository;
    private readonly ITenantContext _tenantContext;

    public CreateMedicinalProductHandler(
        IMedicinalProductPolicy policy,
        IMedicinalProductRepository repository,
        ITenantContext tenantContext)
    {
        _policy = policy;
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task<CreateMedicinalProductResult> HandleAsync(
        CreateMedicinalProductCommand command,
        CancellationToken cancellationToken)
    {
        await _policy.EnsureCanCreateAsync(
            command.GlobalProductId, command.CountryId, cancellationToken);

        // Nothing checks whether this pair already exists, and that is the
        // design: several medicinal products per (global product, country) is a
        // supported business case, so a second one is a decision the user is
        // entitled to make rather than a duplicate to refuse.
        var medicinalProduct = MedicinalProduct.Create(
            _tenantContext.TenantId,
            command.GlobalProductId,
            command.CountryId,
            command.StatusDate);

        await _repository.AddAsync(medicinalProduct, cancellationToken);

        return new CreateMedicinalProductResult(medicinalProduct.Id);
    }
}
