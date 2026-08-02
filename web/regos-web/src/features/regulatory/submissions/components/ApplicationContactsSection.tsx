import { useApplicationContacts } from "../hooks/useApplicationContacts";
import { sequenceLabel } from "../utils/sequenceLabel";

interface Props {
  applicationId: string;
}

/**
 * **Who currently speaks for this application — derived, never stored**
 * (ADR-048).
 *
 * There is no `ApplicationContact` to read. Under the cumulative model
 * (ADR-045) the latest published sequence *is* the current regulatory state, so
 * this is read from it — and the sequence it came from is shown, because a
 * derived answer should say what it was derived from.
 *
 * It lives in the submissions feature rather than in applications: the fact
 * belongs to a filing, and the application page is only where it is read.
 */
export function ApplicationContactsSection({ applicationId }: Props) {
  const { data, isLoading } = useApplicationContacts(applicationId);

  if (isLoading || !data) {
    return <p className="text-sm text-muted-foreground">Loading…</p>;
  }

  // Nothing published: there is no filing, so there is nobody named on one.
  // Worded as an absence of a filing rather than as missing data.
  if (data.asOfSequenceNumber === null) {
    return (
      <p className="text-sm text-muted-foreground" data-testid="no-application-contacts">
        Nothing has been published yet, so nobody has been named on a filing.
      </p>
    );
  }

  return (
    <div className="space-y-3" data-testid="application-contacts">
      <p className="text-sm text-muted-foreground">
        As filed in {sequenceLabel(data.asOfSequenceNumber)}.
      </p>

      {data.contacts.length === 0 ? (
        <p className="text-sm text-muted-foreground">
          That sequence named nobody.
        </p>
      ) : (
        <ol className="divide-y rounded-lg border">
          {data.contacts.map((contact) => (
            <li
              key={`${contact.contactId}-${contact.roleId}`}
              className="p-4"
              data-testid="application-contact"
              data-role={contact.roleName}
            >
              <div className="font-medium">{contact.contactName}</div>

              <p className="text-sm text-muted-foreground">
                {contact.roleName} · {contact.organizationName}
              </p>
            </li>
          ))}
        </ol>
      )}
    </div>
  );
}
