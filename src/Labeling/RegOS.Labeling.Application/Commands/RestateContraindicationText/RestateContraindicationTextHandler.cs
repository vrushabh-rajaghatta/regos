using RegOS.Labeling.Domain.Aggregates.Contraindications;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Labeling.Application.Commands.RestateContraindicationText;

public sealed class RestateContraindicationTextHandler
{
    private readonly IContraindicationRepository _statements;

    public RestateContraindicationTextHandler(IContraindicationRepository statements)
    {
        _statements = statements;
    }

    public async Task HandleAsync(
        RestateContraindicationTextCommand command,
        CancellationToken cancellationToken)
    {
        var statement = await _statements.GetByIdAsync(
                command.ContraindicationId, cancellationToken)
            ?? throw new NotFoundException(
                LabelingRuleErrors.ClinicalStatementDoesNotExist);

        statement.RestateLabelText(command.LabelText);

        await _statements.UpdateAsync(statement, cancellationToken);
    }
}
