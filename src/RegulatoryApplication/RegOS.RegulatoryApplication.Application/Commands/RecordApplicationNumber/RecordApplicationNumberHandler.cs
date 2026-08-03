using RegOS.RegulatoryApplication.Application.Services;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.RegulatoryApplication.Application.Commands.RecordApplicationNumber;

/// <summary>
/// Records the number an authority assigned to an application — <b>the first
/// mutation this context has ever had.</b>
/// </summary>
/// <remarks>
/// <b>The property existed for the whole life of the project and could never be
/// given a value.</b> It was declared with a private setter, absent from
/// <c>Create</c>, mapped by EF, projected by a query and rendered by nothing —
/// so all 59 applications in the development database held null, and no code
/// path existed that could change one. It surfaced only when eCTD generation
/// asked *"where does this fact come from?"* and the answer was *nowhere*.
/// <para>
/// <b>A persistent property with no acquisition path is incomplete modelling</b>,
/// which is a stronger statement than "unused field": the field existed, EF
/// mapped it, and a query projected it. None of that meant the system could ever
/// know its value.
/// </para>
/// </remarks>
public sealed class RecordApplicationNumberHandler
{
    private readonly IRegulatoryApplicationRepository _repository;
    private readonly IApplicationNumberPolicy _policy;

    public RecordApplicationNumberHandler(
        IRegulatoryApplicationRepository repository,
        IApplicationNumberPolicy policy)
    {
        _repository = repository;
        _policy = policy;
    }

    public async Task HandleAsync(
        RecordApplicationNumberCommand command,
        CancellationToken cancellationToken)
    {
        var application = await _repository.GetByIdAsync(
            command.ApplicationId, cancellationToken)
            ?? throw new NotFoundException(
                ApplicationErrors.ApplicationDoesNotExist);

        // The rule the aggregate cannot enforce, because it is a fact about
        // submissions and aggregates reference each other by id only (ES-014).
        // The same division S003 drew for its two handler-resident rules.
        await _policy.EnsureTheNumberCanStillChangeAsync(
            application, command.ApplicationNumber, cancellationToken);

        application.RecordApplicationNumber(command.ApplicationNumber);

        await _repository.SaveChangesAsync(cancellationToken);
    }
}
