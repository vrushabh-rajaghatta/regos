import { useState } from "react";

import { Button } from "@/components/ui/button";

import { useRemoveOrganizationIdentifier } from "../hooks/useRemoveOrganizationIdentifier";
import type { OrganizationIdentifier } from "../types/OrganizationIdentifier";
import { AddOrganizationIdentifierDialog } from "./AddOrganizationIdentifierDialog";

interface OrganizationIdentifiersProps {
  organizationId: string;
  identifiers: OrganizationIdentifier[];
}

/**
 * The registry identifiers a company is known by, and the two things that can
 * happen to them: one is issued, or one is withdrawn. There is no edit —
 * correcting a DUNS number means withdrawing the wrong one and recording the
 * right one, which is what the registry did.
 */
export function OrganizationIdentifiers({
  organizationId,
  identifiers,
}: OrganizationIdentifiersProps) {
  const [addOpen, setAddOpen] = useState(false);
  const remove = useRemoveOrganizationIdentifier(organizationId);

  return (
    <div className="space-y-4" data-testid="organization-identifiers">
      <div className="flex items-center justify-between">
        <p className="text-sm text-muted-foreground">
          {identifiers.length === 0
            ? "No identifiers recorded."
            : `${identifiers.length} recorded`}
        </p>

        <Button variant="outline" size="sm" onClick={() => setAddOpen(true)}>
          Record Identifier
        </Button>
      </div>

      <AddOrganizationIdentifierDialog
        organizationId={organizationId}
        open={addOpen}
        onOpenChange={setAddOpen}
      />

      {remove.isError && (
        <p className="text-sm text-destructive" role="alert">
          {remove.error.message}
        </p>
      )}

      {identifiers.length > 0 && (
        <ul className="divide-y rounded-md border">
          {identifiers.map((identifier) => (
            <li
              key={identifier.id}
              className="flex items-center justify-between gap-4 px-4 py-3"
              data-testid="organization-identifier"
            >
              <div>
                <p className="font-medium">
                  <span title={identifier.schemeName}>
                    {identifier.schemeCode}
                  </span>{" "}
                  <span className="font-mono">{identifier.value}</span>
                </p>
              </div>

              <Button
                variant="ghost"
                size="sm"
                disabled={remove.isPending}
                onClick={() => remove.mutate(identifier.id)}
              >
                Withdraw
              </Button>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
