using RegOS.Interaction.Domain.Correspondence;
using RegOS.Platform.Contracts;
using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.Registration.Domain.Aggregates.Registration;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;

namespace RegOS.Interaction.Application.Commands.GiveCommitment;

public sealed record GiveCommitmentCommand(
    AuthorityId AuthorityId,
    string Title,
    DateOnly GivenOn,
    DateOnly DueOn,
    string? Description,
    UserId? OwnerUserId,
    RegistrationId? RegistrationId,
    RegulatoryApplicationId? RegulatoryApplicationId,
    HaCorrespondenceId? SourceCorrespondenceId);
