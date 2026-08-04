using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Product.Application.Services;
using RegOS.Product.Domain.Product;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Product.Application.Commands.AddIngredient;

public sealed class AddIngredientHandler
{
    private readonly RegOSDbContext _dbContext;
    private readonly IPharmaceuticalProductDetailRepository _presentations;

    public AddIngredientHandler(
        RegOSDbContext dbContext,
        IPharmaceuticalProductDetailRepository presentations)
    {
        _dbContext = dbContext;
        _presentations = presentations;
    }

    public async Task<AddIngredientResult> HandleAsync(
        AddIngredientCommand command,
        CancellationToken cancellationToken)
    {
        var presentation = await _presentations.GetByIdAsync(
                command.PresentationId, cancellationToken)
            ?? throw new NotFoundException(
                PharmaceuticalProductDetailErrors.NotFound);

        // Read directly rather than through a repository: Product owns no
        // Substance repository and should not (ADR-016 — a read is a read).
        // The shared-plus-extensible filter decides what this tenant can see,
        // so a substance belonging to another tenant is simply not found.
        var substanceExists = await _dbContext.Substances
            .AsNoTracking()
            .AnyAsync(x => x.Id == command.SubstanceId, cancellationToken);

        if (!substanceExists)
            throw new NotFoundException(ProductRuleErrors.SubstanceDoesNotExist);

        var ingredient = presentation.AddIngredient(
            command.SubstanceId,
            command.Role,
            StrengthFromCodes.Create(
                command.NumeratorValue,
                command.NumeratorUnitCode,
                command.DenominatorValue,
                command.DenominatorUnitCode));

        await _presentations.UpdateAsync(presentation, cancellationToken);

        return new AddIngredientResult(ingredient.Id);
    }
}
