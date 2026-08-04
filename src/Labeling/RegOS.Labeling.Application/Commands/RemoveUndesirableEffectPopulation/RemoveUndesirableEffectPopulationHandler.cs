using RegOS.Labeling.Domain.Aggregates.UndesirableEffects;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Labeling.Application.Commands.RemoveUndesirableEffectPopulation;

public sealed class RemoveUndesirableEffectPopulationHandler
{
    private readonly IUndesirableEffectRepository _statements;

    public RemoveUndesirableEffectPopulationHandler(IUndesirableEffectRepository statements)
    {
        _statements = statements;
    }

    public async Task HandleAsync(
        RemoveUndesirableEffectPopulationCommand command,
        CancellationToken cancellationToken)
    {
        var statement = await _statements.GetByIdAsync(
                command.UndesirableEffectId, cancellationToken)
            ?? throw new NotFoundException(
                LabelingRuleErrors.ClinicalStatementDoesNotExist);

        statement.RemovePopulation(command.PopulationId);

        await _statements.UpdateAsync(statement, cancellationToken);
    }
}
