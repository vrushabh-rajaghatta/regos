using RegOS.Labeling.Domain.Aggregates.UndesirableEffects;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Labeling.Application.Commands.AddUndesirableEffectPopulation;

public sealed class AddUndesirableEffectPopulationHandler
{
    private readonly IUndesirableEffectRepository _statements;

    public AddUndesirableEffectPopulationHandler(IUndesirableEffectRepository statements)
    {
        _statements = statements;
    }

    public async Task HandleAsync(
        AddUndesirableEffectPopulationCommand command,
        CancellationToken cancellationToken)
    {
        var statement = await _statements.GetByIdAsync(
                command.UndesirableEffectId, cancellationToken)
            ?? throw new NotFoundException(
                LabelingRuleErrors.ClinicalStatementDoesNotExist);

        statement.AddPopulation(
            command.AgeLow,
            command.AgeHigh,
            command.AgeUnitCode,
            command.GenderCode,
            command.PhysiologicalConditionCode,
            command.Description);

        await _statements.UpdateAsync(statement, cancellationToken);
    }
}
