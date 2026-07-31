using RegOS.Organization.Domain.Aggregates.Contact;

namespace RegOS.Organization.Application.Queries.Contacts.GetContact;

public sealed record GetContactQuery(ContactId ContactId);
