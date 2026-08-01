import { zodResolver } from "@hookform/resolvers/zod";
import { Controller, useForm, useWatch } from "react-hook-form";

import { Button } from "@/components/ui/button";
import {
  Field,
  FieldError,
  FieldGroup,
  FieldLabel,
} from "@/components/ui/field";
import { Input } from "@/components/ui/input";

import { useAuthorities } from "../../masterData/hooks/useAuthorities";
import { CORRESPONDENCE_DIRECTIONS } from "../constants/correspondenceDirections";
import { useAuthorityDivisions } from "../hooks/useAuthorityDivisions";
import { useCorrespondenceTypes } from "../hooks/useCorrespondenceTypes";
import { useRecordCorrespondence } from "../hooks/useRecordCorrespondence";
import {
  recordCorrespondenceSchema,
  type RecordCorrespondenceFormValues,
} from "../validation/recordCorrespondenceSchema";

interface RecordCorrespondenceFormProps {
  onSuccess(): void;
}

/**
 * Logging a letter that has already happened. Nothing here defaults to today —
 * the date on the letter is a fact about the letter, and a portfolio carried
 * over from a mailbox is mostly historic.
 */
export function RecordCorrespondenceForm({
  onSuccess,
}: RecordCorrespondenceFormProps) {
  const authorities = useAuthorities();
  const types = useCorrespondenceTypes();
  const mutation = useRecordCorrespondence();

  const {
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<RecordCorrespondenceFormValues>({
    resolver: zodResolver(recordCorrespondenceSchema),
    defaultValues: {
      authorityId: "",
      correspondenceTypeId: "",
      direction: "Inbound",
      subject: "",
      occurredOn: "",
      responseDueOn: "",
      authorityReference: "",
      authorityDivisionId: "",
    },
  });

  // useWatch, not watch(): the memoisation-safe API. The division list depends
  // on the chosen authority, because a division only means anything inside one.
  const selectedAuthorityId = useWatch({ control, name: "authorityId" });
  const divisions = useAuthorityDivisions(selectedAuthorityId ?? "");

  async function onSubmit(values: RecordCorrespondenceFormValues) {
    try {
      await mutation.mutateAsync({
        authorityId: values.authorityId,
        correspondenceTypeId: values.correspondenceTypeId,
        direction: values.direction,
        subject: values.subject,
        occurredOn: values.occurredOn,
        responseDueOn: values.responseDueOn || null,
        authorityReference: values.authorityReference || null,
        authorityDivisionId: values.authorityDivisionId || null,
      });
    } catch {
      // A refusal is an outcome, not a crash. The server's reason renders
      // below and the form keeps what was typed.
      return;
    }

    reset();
    onSuccess();
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
      <FieldGroup>
        <Controller
          control={control}
          name="authorityId"
          render={({ field }) => (
            <Field data-invalid={!!errors.authorityId}>
              <FieldLabel htmlFor="correspondenceAuthorityId">
                Health authority
              </FieldLabel>

              <select
                id="correspondenceAuthorityId"
                className="h-9 w-full rounded-md border bg-transparent px-3 text-sm"
                {...field}
              >
                <option value="">Select an authority</option>
                {(authorities.data ?? []).map((authority) => (
                  <option key={authority.id} value={authority.id}>
                    {authority.name}
                  </option>
                ))}
              </select>

              {errors.authorityId && (
                <FieldError>{errors.authorityId.message}</FieldError>
              )}
            </Field>
          )}
        />

        <Controller
          control={control}
          name="authorityDivisionId"
          render={({ field }) => (
            <Field data-invalid={!!errors.authorityDivisionId}>
              <FieldLabel htmlFor="correspondenceAuthorityDivisionId">
                Division (optional)
              </FieldLabel>

              <select
                id="correspondenceAuthorityDivisionId"
                className="h-9 w-full rounded-md border bg-transparent px-3 text-sm"
                disabled={!selectedAuthorityId}
                {...field}
              >
                <option value="">
                  {selectedAuthorityId
                    ? "Not stated on the letter"
                    : "Choose an authority first"}
                </option>
                {(divisions.data ?? []).map((division) => (
                  <option key={division.id} value={division.id}>
                    {division.name}
                    {division.isTenantDefined ? " (added by us)" : ""}
                  </option>
                ))}
              </select>

              {errors.authorityDivisionId && (
                <FieldError>{errors.authorityDivisionId.message}</FieldError>
              )}
            </Field>
          )}
        />

        <Controller
          control={control}
          name="direction"
          render={({ field }) => (
            <Field data-invalid={!!errors.direction}>
              <FieldLabel htmlFor="correspondenceDirection">
                Received or sent
              </FieldLabel>

              <select
                id="correspondenceDirection"
                className="h-9 w-full rounded-md border bg-transparent px-3 text-sm"
                {...field}
              >
                {CORRESPONDENCE_DIRECTIONS.map((direction) => (
                  <option key={direction.value} value={direction.value}>
                    {direction.label}
                  </option>
                ))}
              </select>

              {errors.direction && (
                <FieldError>{errors.direction.message}</FieldError>
              )}
            </Field>
          )}
        />

        <Controller
          control={control}
          name="correspondenceTypeId"
          render={({ field }) => (
            <Field data-invalid={!!errors.correspondenceTypeId}>
              {/* "Correspondence type", not "Type": the list page behind this
                  dialog has a "Filter by type" control, and while the dialog is
                  open both are in the accessibility tree. One word doing two
                  jobs is a wording defect
                  (docs/engineering/accessible-names.md). */}
              <FieldLabel htmlFor="correspondenceTypeId">
                Correspondence type
              </FieldLabel>

              <select
                id="correspondenceTypeId"
                className="h-9 w-full rounded-md border bg-transparent px-3 text-sm"
                {...field}
              >
                <option value="">Select a type</option>
                {(types.data ?? []).map((type) => (
                  <option key={type.id} value={type.id}>
                    {type.name}
                  </option>
                ))}
              </select>

              {errors.correspondenceTypeId && (
                <FieldError>{errors.correspondenceTypeId.message}</FieldError>
              )}
            </Field>
          )}
        />

        <Controller
          control={control}
          name="subject"
          render={({ field }) => (
            <Field data-invalid={!!errors.subject}>
              <FieldLabel htmlFor="correspondenceSubject">Subject</FieldLabel>

              <Input id="correspondenceSubject" {...field} />

              {errors.subject && (
                <FieldError>{errors.subject.message}</FieldError>
              )}
            </Field>
          )}
        />

        <Controller
          control={control}
          name="occurredOn"
          render={({ field }) => (
            <Field data-invalid={!!errors.occurredOn}>
              {/* "Dated", not "Date": the page also shows a recorded date, and
                  one word for two dates is a page that cannot be read aloud
                  unambiguously. */}
              <FieldLabel htmlFor="correspondenceOccurredOn">Dated</FieldLabel>

              <Input id="correspondenceOccurredOn" type="date" {...field} />

              {errors.occurredOn && (
                <FieldError>{errors.occurredOn.message}</FieldError>
              )}
            </Field>
          )}
        />

        <Controller
          control={control}
          name="responseDueOn"
          render={({ field }) => (
            <Field data-invalid={!!errors.responseDueOn}>
              <FieldLabel htmlFor="correspondenceResponseDueOn">
                Response due (optional)
              </FieldLabel>

              <Input id="correspondenceResponseDueOn" type="date" {...field} />

              {errors.responseDueOn && (
                <FieldError>{errors.responseDueOn.message}</FieldError>
              )}
            </Field>
          )}
        />

        <Controller
          control={control}
          name="authorityReference"
          render={({ field }) => (
            <Field data-invalid={!!errors.authorityReference}>
              <FieldLabel htmlFor="correspondenceAuthorityReference">
                Authority reference (optional)
              </FieldLabel>

              <Input id="correspondenceAuthorityReference" {...field} />

              {errors.authorityReference && (
                <FieldError>{errors.authorityReference.message}</FieldError>
              )}
            </Field>
          )}
        />
      </FieldGroup>

      {mutation.isError && (
        <p
          className="text-sm text-destructive"
          data-testid="record-correspondence-error"
        >
          {(mutation.error as Error).message}
        </p>
      )}

      <div className="flex justify-end">
        {/* "Log", not "Log correspondence": the page's own action carries the
            full name, and two controls with one accessible name is a wording
            defect (docs/engineering/accessible-names.md). */}
        <Button type="submit" disabled={mutation.isPending}>
          {mutation.isPending ? "Logging..." : "Log"}
        </Button>
      </div>
    </form>
  );
}
