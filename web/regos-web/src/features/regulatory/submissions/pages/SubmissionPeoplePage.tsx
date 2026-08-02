import { useState } from "react";
import { useParams } from "react-router-dom";

import { Button } from "@/components/ui/button";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { useContactDirectory } from "@/features/regulatory/organizations/hooks/useContactDirectory";
import { useContactRoles } from "@/features/regulatory/organizations/hooks/useContactRoles";

import { useAssignSubmissionRole } from "../hooks/useAssignSubmissionRole";
import { useRemoveSubmissionRole } from "../hooks/useRemoveSubmissionRole";
import { useSubmission } from "../hooks/useSubmission";
import { useSubmissionRoles } from "../hooks/useSubmissionRoles";
import { sequenceLabel } from "../utils/sequenceLabel";

/**
 * **Who is named on this filing** (ADR-048).
 *
 * Editable while a draft and a plain record afterwards, exactly like the
 * format: who was named on sequence 0003 is a fact about a filing already made.
 *
 * The role names come from the same `ContactRole` reference data a contact's
 * own roles draw on — shared vocabulary, separate fact. Naming somebody as
 * Qualified Person here does not require their profile to say so.
 */
export function SubmissionPeoplePage() {
  const { submissionId } = useParams();

  const { data: submission } = useSubmission(submissionId!);
  const { data: roles, isLoading } = useSubmissionRoles(submissionId!);

  const { data: directory } = useContactDirectory();
  const { data: contactRoles } = useContactRoles();

  const assign = useAssignSubmissionRole(submissionId!);
  const remove = useRemoveSubmissionRole(submissionId!);

  const [contactId, setContactId] = useState("");
  const [roleId, setRoleId] = useState("");

  if (isLoading || !submission) {
    return <div className="p-6 text-muted-foreground">Loading…</div>;
  }

  const isDraft = submission.status === "Draft";

  // Deactivated contacts are not offered: "do not name this person on anything
  // new" is what deactivation means, and the server refuses it anyway.
  const selectable = (directory ?? []).filter((c) => c.status === "Active");

  const contactItems = Object.fromEntries(
    selectable.map((c) => [c.contactId, `${c.firstName} ${c.lastName}`])
  );

  const roleItems = Object.fromEntries(
    (contactRoles ?? []).map((r) => [r.id, r.name])
  );

  return (
    <div className="max-w-3xl space-y-8 p-6">
      <section className="space-y-1">
        <h2 className="text-lg font-semibold">Who is named on this filing</h2>

        <p className="text-sm text-muted-foreground">
          {isDraft
            ? "Editable while this is a draft."
            : `Fixed when ${sequenceLabel(
                submission.sequenceNumber ?? 0
              )} was published.`}
        </p>
      </section>

      {/* Rendered because its absence hid a real defect: a failing removal
          looked exactly like a successful one that had not refreshed. */}
      {remove.isError && (
        <p className="text-sm text-destructive" data-testid="remove-role-error">
          {remove.error.message}
        </p>
      )}

      {roles && roles.length > 0 ? (
        <ol className="divide-y rounded-lg border">
          {roles.map((role) => (
            <li
              key={role.id}
              className="flex items-center justify-between gap-4 p-4"
              data-testid="submission-role"
              data-role={role.roleName}
            >
              <div>
                <div className="font-medium">{role.contactName}</div>

                <p className="text-sm text-muted-foreground">
                  {role.roleName} · {role.organizationName}
                  {role.contactTitle ? ` · ${role.contactTitle}` : ""}
                </p>
              </div>

              {isDraft && (
                <Button
                  variant="ghost"
                  size="sm"
                  data-testid="remove-submission-role"
                  disabled={remove.isPending}
                  onClick={() => remove.mutate(role.id)}
                >
                  Remove
                </Button>
              )}
            </li>
          ))}
        </ol>
      ) : (
        // Not an error state: a sequence that names nobody is unusual, not
        // invalid, and nothing requires a role to be present to publish.
        <p
          className="rounded-lg border p-4 text-sm text-muted-foreground"
          data-testid="no-submission-roles"
        >
          Nobody is named on this filing.
        </p>
      )}

      {isDraft && (
        <section className="space-y-3 border-t pt-8">
          <h3 className="text-xs font-semibold uppercase tracking-wide text-muted-foreground">
            Name someone
          </h3>

          <div className="flex flex-wrap items-end gap-3">
            <div className="min-w-56 flex-1">
              <Select
                items={contactItems}
                value={contactId}
                onValueChange={(next) => setContactId(next ?? "")}
              >
                <SelectTrigger id="contactId" className="w-full">
                  <SelectValue placeholder="Select a person" />
                </SelectTrigger>

                <SelectContent>
                  {selectable.map((contact) => (
                    <SelectItem key={contact.contactId} value={contact.contactId}>
                      {contact.firstName} {contact.lastName} ·{" "}
                      {contact.organizationName}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="min-w-56 flex-1">
              <Select
                items={roleItems}
                value={roleId}
                onValueChange={(next) => setRoleId(next ?? "")}
              >
                <SelectTrigger id="roleId" className="w-full">
                  <SelectValue placeholder="Select a role" />
                </SelectTrigger>

                <SelectContent>
                  {(contactRoles ?? []).map((role) => (
                    <SelectItem key={role.id} value={role.id}>
                      {role.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <Button
              data-testid="assign-submission-role"
              disabled={!contactId || !roleId || assign.isPending}
              onClick={() =>
                assign.mutate(
                  { contactId, roleId },
                  {
                    onSuccess: () => {
                      setContactId("");
                      setRoleId("");
                    },
                  }
                )
              }
            >
              {assign.isPending ? "Naming…" : "Name on filing"}
            </Button>
          </div>

          {assign.isError && (
            <p className="text-sm text-destructive" data-testid="assign-role-error">
              {assign.error.message}
            </p>
          )}
        </section>
      )}
    </div>
  );
}
