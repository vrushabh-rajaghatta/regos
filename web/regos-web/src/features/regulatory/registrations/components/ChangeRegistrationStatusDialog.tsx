import { useState } from "react";

import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Field, FieldLabel } from "@/components/ui/field";
import { Input } from "@/components/ui/input";

import { statusLabel } from "../constants/statusLabel";
import { useChangeRegistrationStatus } from "../hooks/useChangeRegistrationStatus";
import { useRecordRegistrationApproval } from "../hooks/useRecordRegistrationApproval";
import type { RegistrationDetail } from "../types/RegistrationDetail";

interface Props {
  registration: RegistrationDetail;
  /** The status being moved to. */
  target: string;
  onClose: () => void;
}

/**
 * One dialog for every transition the server offered.
 *
 * It chooses between two operations by reading the record rather than by
 * knowing the lifecycle: a registration with no number yet has never been
 * granted, so becoming Approved is the grant and needs the number and validity
 * dates. Returning to Approved from Suspended is a lift, and needs neither.
 */
/**
 * Mounted by its caller only while a transition is being recorded, and
 * unmounted the moment it closes. That is what keeps each transition's form
 * fresh — a half-typed date never carries into an unrelated one — without
 * clearing fields one by one, and it leaves no empty shell behind on the way
 * out.
 */
export function ChangeRegistrationStatusDialog({
  registration,
  target,
  onClose,
}: Props) {
  return (
    <Dialog open onOpenChange={(open) => !open && onClose()}>
      <DialogContent>
        <TransitionForm
          registration={registration}
          target={target}
          onDone={onClose}
          onCancel={onClose}
        />
      </DialogContent>
    </Dialog>
  );
}

function TransitionForm({
  registration,
  target,
  onDone,
  onCancel,
}: {
  registration: RegistrationDetail;
  target: string;
  onDone: () => void;
  onCancel: () => void;
}) {
  const isGrant =
    target === "Approved" && registration.registrationNumber === null;

  const [occurredOn, setOccurredOn] = useState("");
  const [note, setNote] = useState("");
  const [registrationNumber, setRegistrationNumber] = useState("");
  const [expiresOn, setExpiresOn] = useState("");

  const changeStatus = useChangeRegistrationStatus(registration.id);
  const recordApproval = useRecordRegistrationApproval(registration.id);

  const mutation = isGrant ? recordApproval : changeStatus;

  async function submit(event: React.FormEvent) {
    event.preventDefault();

    try {
      if (isGrant) {
        await recordApproval.mutateAsync({
          registrationNumber,
          approvedOn: occurredOn,
          expiresOn: expiresOn === "" ? null : expiresOn,
          note: note === "" ? null : note,
        });
      } else {
        await changeStatus.mutateAsync({
          status: target,
          occurredOn,
          note: note === "" ? null : note,
        });
      }

      onDone();
    } catch {
      // A refusal is an outcome, not a crash. The mutation holds the message
      // and the form below renders it; the dialog stays open so the date or
      // number can be corrected without retyping the rest.
    }
  }

  return (
    <>
      <DialogHeader>
        <DialogTitle>
          {isGrant
            ? "Record the grant"
            : `Change status to ${statusLabel(target)}`}
        </DialogTitle>
      </DialogHeader>

      <form onSubmit={submit} className="space-y-4">
        {isGrant && (
          <Field>
            <FieldLabel htmlFor="registrationNumber">
              Registration number
            </FieldLabel>
            <Input
              id="registrationNumber"
              required
              value={registrationNumber}
              onChange={(event) => setRegistrationNumber(event.target.value)}
              placeholder="NDA-123456"
            />
          </Field>
        )}

        <Field>
          {/*
            The business date, never today's: an authorisation granted in 2019
            and entered now must record 2019.
          */}
          <FieldLabel htmlFor="occurredOn">
            {isGrant ? "Approved on" : "Took effect on"}
          </FieldLabel>
          <Input
            id="occurredOn"
            type="date"
            required
            value={occurredOn}
            onChange={(event) => setOccurredOn(event.target.value)}
          />
        </Field>

        {isGrant && (
          <Field>
            <FieldLabel htmlFor="expiresOn">Expires on (optional)</FieldLabel>
            <Input
              id="expiresOn"
              type="date"
              value={expiresOn}
              onChange={(event) => setExpiresOn(event.target.value)}
            />
          </Field>
        )}

        <Field>
          <FieldLabel htmlFor="note">Note (optional)</FieldLabel>
          <Input
            id="note"
            value={note}
            onChange={(event) => setNote(event.target.value)}
            placeholder="Suspension lifted."
          />
        </Field>

        {/*
          The server's own words. Its refusals are written for a regulatory
          reader, so they are shown rather than paraphrased.
        */}
        {mutation.isError && (
          <p
            className="text-sm text-destructive"
            data-testid="status-change-error"
          >
            {(mutation.error as Error).message}
          </p>
        )}

        <div className="flex justify-end gap-2">
          <Button type="button" variant="outline" onClick={onCancel}>
            Cancel
          </Button>

          <Button type="submit" disabled={mutation.isPending}>
            {mutation.isPending ? "Saving..." : "Save"}
          </Button>
        </div>
      </form>
    </>
  );
}
