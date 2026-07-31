import { useMemo, useState } from "react";

import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Field, FieldLabel } from "@/components/ui/field";
import { Input } from "@/components/ui/input";

import { useAuthorities } from "../../masterData/hooks/useAuthorities";
import { useOrganizations } from "../../masterData/hooks/useOrganizations";
import type { MedicinalProduct } from "../../medicinalProducts/types/MedicinalProduct";
import { useCreateRegistration } from "../hooks/useCreateRegistration";

interface Props {
  market: MedicinalProduct;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

/**
 * A registration always begins Planned, whatever its real history. An
 * authorisation granted in 2019 is created with its 2019 date and then has its
 * grant recorded — which is why this form asks when it entered its first status
 * rather than assuming today.
 *
 * It no longer asks for a market. The market is the medicinal product this
 * licence is being granted over, chosen before the dialog opened, and there is
 * no longer any way to state one that disagrees with it.
 */
export function CreateRegistrationDialog({
  market,
  open,
  onOpenChange,
}: Props) {
  const [authorityId, setAuthorityId] = useState("");
  const [holderOrganizationId, setHolderOrganizationId] = useState("");
  const [occurredOn, setOccurredOn] = useState("");
  const [note, setNote] = useState("");

  const authorities = useAuthorities();
  const organizations = useOrganizations();
  const mutation = useCreateRegistration(market.medicinalProductId);

  // The server rejects an authority that does not regulate the market's
  // country, so the choice is narrowed here rather than offering one that
  // cannot work. The country comes from the market, not from a field.
  const authoritiesInCountry = useMemo(
    () => (authorities.data ?? []).filter((a) => a.countryId === market.countryId),
    [authorities.data, market.countryId]
  );

  const activeHolders = useMemo(
    () => (organizations.data ?? []).filter((o) => o.status === "Active"),
    [organizations.data]
  );

  async function submit(event: React.FormEvent) {
    event.preventDefault();

    try {
      await mutation.mutateAsync({
        authorityId,
        holderOrganizationId,
        occurredOn,
        note: note === "" ? null : note,
      });
    } catch {
      // A refusal is an outcome, not a crash — the server's reason is rendered
      // below and the form keeps what was typed.
      return;
    }

    setAuthorityId("");
    setHolderOrganizationId("");
    setOccurredOn("");
    setNote("");
    onOpenChange(false);
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>New registration in {market.countryName}</DialogTitle>
        </DialogHeader>

        <form onSubmit={submit} className="space-y-4">
          <Field>
            <FieldLabel htmlFor="authorityId">Authority</FieldLabel>
            <select
              id="authorityId"
              required
              value={authorityId}
              onChange={(event) => setAuthorityId(event.target.value)}
              className="h-9 w-full rounded-md border bg-transparent px-3 text-sm"
            >
              <option value="">Select an authority</option>
              {authoritiesInCountry.map((authority) => (
                <option key={authority.id} value={authority.id}>
                  {authority.name}
                </option>
              ))}
            </select>
          </Field>

          <Field>
            <FieldLabel htmlFor="holderOrganizationId">
              Authorisation holder
            </FieldLabel>
            <select
              id="holderOrganizationId"
              required
              value={holderOrganizationId}
              onChange={(event) =>
                setHolderOrganizationId(event.target.value)
              }
              className="h-9 w-full rounded-md border bg-transparent px-3 text-sm"
            >
              <option value="">Select an organisation</option>
              {activeHolders.map((organization) => (
                <option key={organization.id} value={organization.id}>
                  {organization.legalName}
                </option>
              ))}
            </select>
          </Field>

          <Field>
            <FieldLabel htmlFor="occurredOn">Planned on</FieldLabel>
            <Input
              id="occurredOn"
              type="date"
              required
              value={occurredOn}
              onChange={(event) => setOccurredOn(event.target.value)}
            />
          </Field>

          <Field>
            <FieldLabel htmlFor="createNote">Note (optional)</FieldLabel>
            <Input
              id="createNote"
              value={note}
              onChange={(event) => setNote(event.target.value)}
              placeholder="Carried over from the legacy register."
            />
          </Field>

          {mutation.isError && (
            <p
              className="text-sm text-destructive"
              data-testid="create-registration-error"
            >
              {(mutation.error as Error).message}
            </p>
          )}

          <div className="flex justify-end gap-2">
            <Button
              type="button"
              variant="outline"
              onClick={() => onOpenChange(false)}
            >
              Cancel
            </Button>

            <Button type="submit" disabled={mutation.isPending}>
              {mutation.isPending ? "Creating..." : "Create"}
            </Button>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  );
}
