using RegOS.Submission.Application.Validation;
using RegOS.Submission.Domain.Submission;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Submission.Application.Queries.ValidateSubmission;

/// <summary>
/// Exposes <see cref="SubmissionValidator"/> as a query: it orchestrates the call and
/// maps the internal result to the API response. It holds no validation rules of its
/// own — the validator owns readiness, the handler owns the contract.
/// </summary>
public sealed class ValidateSubmissionHandler
{
    private readonly SubmissionValidator _validator;

    public ValidateSubmissionHandler(SubmissionValidator validator)
    {
        _validator = validator;
    }

    /// <summary>
    /// Validates the submission and returns its status. Propagates
    /// <see cref="NotFoundException"/> when the submission does not exist so
    /// the endpoint can 404.
    /// </summary>
    public async Task<ValidateSubmissionResponse> HandleAsync(
        SubmissionId submissionId,
        CancellationToken cancellationToken)
    {
        var result = await _validator.ValidateAsync(submissionId, cancellationToken);

        return ValidateSubmissionResponse.From(result);
    }
}
