import { useState } from "react";
import { Link } from "react-router-dom";

import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { PageHeader } from "@/shared/components/PageHeader";

import { ContactRoleBadges } from "../components/ContactRoleBadges";
import { OrganizationStatusBadge } from "../components/OrganizationStatusBadge";
import { useContactDirectory } from "../hooks/useContactDirectory";
import { useContactRoles } from "../hooks/useContactRoles";

const ANY = "any";

/**
 * "Who holds this role?" — across the registry rather than within one company.
 *
 * The question that made Contact an aggregate root. Nothing is hidden: an
 * inactive contact is returned and marked, because the person named on a 2019
 * licence is still that person.
 */
export function ContactDirectoryPage() {
  const [roleId, setRoleId] = useState(ANY);

  const { data: roles } = useContactRoles();

  const { data: contacts, isPending, error } = useContactDirectory(
    roleId === ANY ? undefined : roleId,
  );

  return (
    <div className="p-6">
      <PageHeader
        title="Contacts"
        description="Every person in the registry, whoever they work for"
      />

      <div className="mt-6">
        {/* The Select clears to null; "any" is this page's word for no filter. */}
        <Select
          value={roleId}
          onValueChange={(value) => setRoleId(value ?? ANY)}
        >
          <SelectTrigger className="w-64" data-testid="contact-role-filter">
            <SelectValue />
          </SelectTrigger>

          <SelectContent>
            <SelectItem value={ANY}>All roles</SelectItem>

            {(roles ?? []).map((role) => (
              <SelectItem key={role.id} value={role.id}>
                {role.name}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      <div className="mt-6">
        {isPending && (
          <p data-testid="contact-directory-loading">Loading contacts...</p>
        )}

        {error && (
          <p data-testid="contact-directory-error">
            Unable to load the directory.
          </p>
        )}

        {contacts && (
          <>
            <p
              className="mb-3 text-sm text-muted-foreground"
              data-testid="contact-directory-count"
            >
              {contacts.length}{" "}
              {contacts.length === 1 ? "contact" : "contacts"}
            </p>

            <ul
              className="divide-y rounded-md border"
              data-testid="contact-directory"
            >
              {contacts.map((contact) => (
                <li
                  key={contact.contactId}
                  className="flex items-start justify-between gap-4 px-4 py-3"
                  data-testid="contact-directory-row"
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

                    <p className="text-sm">
                      <Link
                        to={`/regulatory/organizations/${contact.organizationId}/contacts`}
                        className="hover:underline"
                      >
                        {contact.organizationName}
                      </Link>

                      {contact.siteName && (
                        <span className="text-muted-foreground">
                          {" · "}
                          {contact.siteName}
                        </span>
                      )}
                    </p>

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
          </>
        )}
      </div>
    </div>
  );
}
