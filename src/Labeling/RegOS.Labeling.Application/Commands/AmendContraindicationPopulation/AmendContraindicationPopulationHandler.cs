using RegOS.Labeling.Domain.Aggregates.Contraindications;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Labeling.Application.Commands.AmendContraindicationPopulation;

public sealed class AmendContraindicationPopulationHandler
{
    private readonly IContraindicationRepository _statements;

    public AmendContraindicationPopulationHandler(IContraindicationRepository statements)
    {
        _statements = statements;
    }

    public async Task HandleAsync(
        AmendContraindicationPopulationCommand command,
        CancellationToken cancellationToken)
    {
        var statement = await _statements.GetByIdAsync(
                command.ContraindicationId, cancellationToken)
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
