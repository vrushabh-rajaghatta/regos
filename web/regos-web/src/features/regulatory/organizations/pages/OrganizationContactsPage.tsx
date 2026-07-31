import { useState } from "react";
import { useParams } from "react-router-dom";

import { Button } from "@/components/ui/button";
import { PageHeader } from "@/shared/components/PageHeader";

import { ContactRoleBadges } from "../components/ContactRoleBadges";
import { CreateContactDialog } from "../components/CreateContactDialog";
import { OrganizationStatusBadge } from "../components/OrganizationStatusBadge";
import { useOrganizationContacts } from "../hooks/useOrganizationContacts";

/** The people this company names in regulatory work. */
export function OrganizationContactsPage() {
  const { organizationId } = useParams();
  const [createOpen, setCreateOpen] = useState(false);

  const { data: contacts, isPending, error } = useOrganizationContacts(
    organizationId!,
  );

  return (
    <div className="p-6">
      <PageHeader
        title="Contacts"
        description="People this organization names in regulatory work"
        actions={
          <Button onClick={() => setCreateOpen(true)}>Add Contact</Button>
        }
      />

      <CreateContactDialog
        organizationId={organizationId!}
        open={createOpen}
        onOpenChange={setCreateOpen}
      />

      <div className="mt-6">
        {isPending && <p data-testid="contacts-loading">Loading contacts...</p>}

        {error && <p data-testid="contacts-error">Unable to load contacts.</p>}

        {contacts?.length === 0 && (
          <p className="text-muted-foreground" data-testid="contacts-empty">
            No contacts recorded.
          </p>
        )}

        {contacts && contacts.length > 0 && (
          <ul className="divide-y rounded-md border" data-testid="contact-list">
            {contacts.map((contact) => (
              <li
                key={contact.contactId}
                className="flex items-start justify-between gap-4 px-4 py-3"
                data-testid="contact-row"
              >
                <div className="space-y-1">
                  <p className="font-medium">
                    {contact.firstName} {contact.lastName}
                    {contact.title && (
                      <span className="ml-2 text-sm text-muted-foreground">
                        {contact.title}
                      </span>
                    )}
                  </p>

                  {contact.siteName && (
                    <p className="text-sm text-muted-foreground">
                      {contact.siteName}
                    </p>
                  )}

                  {contact.emails.length > 0 && (
                    <p className="text-sm text-muted-foreground">
                      {contact.emails.join(" · ")}
                    </p>
                  )}

                  <ContactRoleBadges roles={contact.roles} />
                </div>

                <OrganizationStatusBadge status={contact.status} />
              </li>
            ))}
          </ul>
        )}
      </div>
    </div>
  );
}
