using RegOS.ReferenceData.Domain.Substances;
using RegOS.ReferenceData.Domain.Terminology;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.ReferenceData.Application.Commands.CreateSubstance;

/// <summary>
/// The first write path into <c>ReferenceData</c>, and deliberately the only
/// one: <em>create a tenant-owned substance</em> (ADR-058 §5).
/// </summary>
public sealed class CreateSubstanceHandler
{
    private readonly ISubstanceRepository _repository;
    private readonly ITenantContext _tenantContext;

    public CreateSubstanceHandler(
        ISubstanceRepository repository,
        ITenantContext tenantContext)
    {
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task<CreateSubstanceResult> HandleAsync(
        CreateSubstanceCommand command,
        CancellationToken cancellationToken)
    {
        // "Is this a word?" is answered at the boundary, not by the aggregate —
        // the division EPIC-019 settled when the generator, and not the study,
        // refused an identifier a filename could not carry. It also keeps the
        // vocabulary swappable: when licensed terminology arrives, this
        // resolution changes and the domain does not.
        var substanceClass = SubstanceVocabulary.ClassOf(command.SubstanceClassCode)
            ?? throw new DomainException(
                SubstanceVocabularyErrors.UnknownClass(command.SubstanceClassCode));

        var substanceType = SubstanceVocabulary.TypeOf(command.SubstanceTypeCode)
            ?? throw new DomainException(
                SubstanceVocabularyErrors.UnknownType(command.SubstanceTypeCode));

        // Built before the name check so the canonical trimmed name is what is
        // looked up — " Aspirin " and "Aspirin" are one compound. It also puts
        // the shape refusals (400) ahead of the clash (409), which is the order
        // a user can act on.
        var substance = Substance.CreateForTenant(
            _tenantContext.TenantId,
            command.Name,
            command.Inn,
            substanceClass,
            substanceType,
            command.CasNumber,
            command.UniiCode,
            command.MolecularFormula,
            command.Description);

        var existing = await _repository.FindVisibleByNameAsync(
            _tenantContext.TenantId, substance.Name, cancellationToken);

        if (existing is not null)
        {
            throw new BusinessRuleViolationException(
                existing.IsShared
                    ? SubstanceErrors.NameAlreadyInSharedCatalogue
                    : SubstanceErrors.NameAlreadyAdded);
        }

        await _repository.AddAsync(substance, cancellationToken);

        return new CreateSubstanceResult(substance.Id);
    }
}
