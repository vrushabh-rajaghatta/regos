using RegOS.Labeling.Domain.Aggregates.Contraindications;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Labeling.Application.Commands.AddContraindicationPopulation;

public sealed class AddContraindicationPopulationHandler
{
    private readonly IContraindicationRepository _statements;

    public AddContraindicationPopulationHandler(IContraindicationRepository statements)
    {
        _statements = statements;
    }

    public async Task HandleAsync(
        AddContraindicationPopulationCommand command,
        CancellationToken cancellationToken)
    {
        var statement = await _statements.GetByIdAsync(
                command.ContraindicationId, cancellationToken)
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
