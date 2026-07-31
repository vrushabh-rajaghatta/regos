using RegOS.Organization.Domain.Aggregates.Organization;

namespace RegOS.Organization.Application.Commands.RemoveOrganizationIdentifier;

/// <summary>
/// Withdraws an identifier from the company's record.
/// </summary>
/// <remarks>
/// A genuine removal rather than a lifecycle change, which is the exception to
/// ES-018 rather than a contradiction of it: the regulatory record being
/// retained is the organization, and an identifier recorded against the wrong
/// company is a mistake to erase, not a history to keep.
/// </remarks>
public sealed record RemoveOrganizationIdentifierCommand(
    OrganizationId OrganizationId,
    OrganizationIdentifierId IdentifierId);
