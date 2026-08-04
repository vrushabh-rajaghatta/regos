using RegOS.Labeling.Domain.Aggregates.UndesirableEffects;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Labeling.Application.Commands.AmendUndesirableEffectPopulation;

public sealed class AmendUndesirableEffectPopulationHandler
{
    private readonly IUndesirableEffectRepository _statements;

    public AmendUndesirableEffectPopulationHandler(IUndesirableEffectRepository statements)
    {
        _statements = statements;
    }

    public async Task HandleAsync(
        AmendUndesirableEffectPopulationCommand command,
        CancellationToken cancellationToken)
    {
        var statement = await _statements.GetByIdAsync(
                command.UndesirableEffectId, cancellationToken)
            ?? throw new NotFoundException(
                LabelingRuleErrors.ClinicalStatementDoesNotExist);

        statement.AmendPopulation(
            command.PopulationId,
            command.AgeLow,
            command.AgeHigh,
            command.AgeUnitCode,
            command.GenderCode,
            command.PhysiologicalConditionCode,
            command.Description);

        await _statements.UpdateAsync(statement, cancellationToken);
    }
}
