using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.RegulatoryApplication.Application.Commands.StopCitingStudy;

public sealed class StopCitingStudyHandler
{
    private readonly IRegulatoryApplicationRepository _repository;

    public StopCitingStudyHandler(
        IRegulatoryApplicationRepository repository)
    {
        _repository = repository;
    }

    public async Task HandleAsync(
        StopCitingStudyCommand command,
        CancellationToken cancellationToken)
    {
        var application = await _repository.GetByIdAsync(
            command.ApplicationId, cancellationToken);

        if (application is null)
            throw new NotFoundException(
                RegulatoryApplicationErrors.ApplicationDoesNotExist);

        application.StopCitingStudy(command.StudyId);

        await _repository.SaveChangesAsync(cancellationToken);
    }
}
