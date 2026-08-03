using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;

namespace RegOS.RegulatoryApplication.Application.Commands.RecordApplicationNumber;

/// <param name="ApplicationNumber">
/// Exactly as the authority issued it. RegOS does not reshape it — see
/// <see cref="RegulatoryApplication.Domain.Aggregates.RegulatoryApplication.RegulatoryApplication.RecordApplicationNumber"/>.
/// </param>
public sealed record RecordApplicationNumberCommand(
    RegulatoryApplicationId ApplicationId,
    string ApplicationNumber);
