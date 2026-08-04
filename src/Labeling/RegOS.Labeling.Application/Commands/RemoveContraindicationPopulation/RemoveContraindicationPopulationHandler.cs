using RegOS.Labeling.Domain.Aggregates.Contraindications;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Labeling.Application.Commands.RemoveContraindicationPopulation;

public sealed class RemoveContraindicationPopulationHandler
{
    private readonly IContraindicationRepository _statements;

    public RemoveContraindicationPopulationHandler(IContraindicationRepository statements)
    {
        _statements = statements;
    }

    public async Task HandleAsync(
        RemoveContraindicationPopulationCommand command,
        CancellationToken cancellationToken)
    {
        var statement = await _statements.GetByIdAsync(
                command.ContraindicationId, cancellationToken)
            ?? throw new NotFoundException(
                LabelingRuleErrors.ClinicalStatementDoesNotExist);

        statement.RemovePopulation(command.PopulationId);

        await _statements.UpdateAsync(statement, cancellationToken);
    }
}
