using RegOS.Labeling.Domain.Aggregates.UndesirableEffects;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Labeling.Application.Commands.RestateUndesirableEffectText;

public sealed class RestateUndesirableEffectTextHandler
{
    private readonly IUndesirableEffectRepository _statements;

    public RestateUndesirableEffectTextHandler(IUndesirableEffectRepository statements)
    {
        _statements = statements;
    }

    public async Task HandleAsync(
        RestateUndesirableEffectTextCommand command,
        CancellationToken cancellationToken)
    {
        var statement = await _statements.GetByIdAsync(
                command.UndesirableEffectId, cancellationToken)
            ?? throw new NotFoundException(
                LabelingRuleErrors.ClinicalStatementDoesNotExist);

        statement.RestateLabelText(command.LabelText);

        await _statements.UpdateAsync(statement, cancellationToken);
    }
}
