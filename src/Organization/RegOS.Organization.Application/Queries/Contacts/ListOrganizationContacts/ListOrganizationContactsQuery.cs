using RegOS.Organization.Domain.Aggregates.Organization;

namespace RegOS.Organization.Application.Queries.Contacts.ListOrganizationContacts;

/// <summary>"Who do we know at this company?" — the mirror of the directory.</summary>
public sealed record ListOrganizationContactsQuery(OrganizationId OrganizationId);
