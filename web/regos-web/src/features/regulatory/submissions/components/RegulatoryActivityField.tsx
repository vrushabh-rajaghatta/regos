import { Controller, type Control, type FieldErrors } from "react-hook-form";

import {
  Field,
  FieldError,
  FieldLabel,
} from "@/components/ui/field";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";

import { useContinuableSubmissions } from "../hooks/useContinuableSubmissions";
import { useSubmissionSubTypes } from "../hooks/useSubmissionSubTypes";
import { useSubmissionTypes } from "../hooks/useSubmissionTypes";
import type { CreateSubmissionFormValues } from "../validation/createSubmissionSchema";

interface Props {
  control: Control<CreateSubmissionFormValues>;
  errors: FieldErrors<CreateSubmissionFormValues>;
  applicationId: string;
  authorityId: string;
  activityChoice: "start" | "continue";
}

/**
 * The regulatory activity a sequence belongs to, and what it does to it.
 *
 * "Regulatory activity" is the screen's word and appears nowhere in the domain
 * model, which has no Activity concept — an activity is derived from a chain of
 * submissions. Both words are binding, and this is where they meet.
 */
export function RegulatoryActivityField({
  control,
  errors,
  applicationId,
  authorityId,
  activityChoice,
}: Props) {
  const { data: submissionTypes } = useSubmissionTypes(authorityId);
  const { data: subTypes } = useSubmissionSubTypes(authorityId);
  const { data: continuable } = useContinuableSubmissions(applicationId);

  const hasContinuable = (continuable?.length ?? 0) > 0;

  return (
    <>
      <Controller
        control={control}
        name="activityChoice"
        render={({ field }) => (
          <Field>
            <FieldLabel htmlFor="activity-start">Regulatory Activity</FieldLabel>

            <div
              role="radiogroup"
              aria-label="Regulatory Activity"
              className="space-y-2"
            >
              <label
                htmlFor="activity-start"
                className="flex items-start gap-2 text-sm"
              >
                <input
                  id="activity-start"
                  type="radio"
                  className="mt-1"
                  checked={field.value === "start"}
                  onChange={() => field.onChange("start")}
                />

                <span>
                  Start a new regulatory activity
                  <span className="block text-muted-foreground">
                    This sequence opens something new — an original application,
                    an annual report, a safety report.
                  </span>
                </span>
              </label>

              <label
                htmlFor="activity-continue"
                className="flex items-start gap-2 text-sm"
              >
                <input
                  id="activity-continue"
                  type="radio"
                  className="mt-1"
                  disabled={!hasContinuable}
                  checked={field.value === "continue"}
                  onChange={() => field.onChange("continue")}
                />

                <span>
                  Continue an existing regulatory activity
                  {/*
                    Disabled rather than hidden when there is nothing to
                    continue: the option existing tells the user the concept
                    exists, and the reason tells them why it is not available
                    yet. A missing control would just look like an oversight.
                  */}
                  <span className="block text-muted-foreground">
                    {hasContinuable
                      ? "This sequence adds to an activity already opened by a published filing."
                      : "No published filing has opened an activity in this application yet."}
                  </span>
                </span>
              </label>
            </div>
          </Field>
        )}
      />

      {activityChoice === "start" && (
        <Controller
          control={control}
          name="submissionTypeId"
          render={({ field }) => (
            <Field data-invalid={!!errors.submissionTypeId}>
              <FieldLabel htmlFor="submissionTypeId">
                What activity does this start?
              </FieldLabel>

              <Select
                value={field.value ?? ""}
                onValueChange={field.onChange}
              >
                <SelectTrigger id="submissionTypeId" className="w-full">
                  <SelectValue placeholder="Select a regulatory activity" />
                </SelectTrigger>

                <SelectContent>
                  {(submissionTypes ?? []).map((type) => (
                    <SelectItem key={type.id} value={type.id}>
                      {type.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>

              <FieldError errors={[errors.submissionTypeId]} />
            </Field>
          )}
        />
      )}

      {activityChoice === "continue" && (
        <Controller
          control={control}
          name="originatingSubmissionId"
          render={({ field }) => (
            <Field data-invalid={!!errors.originatingSubmissionId}>
              <FieldLabel htmlFor="originatingSubmissionId">
                Which activity does this continue?
              </FieldLabel>

              <Select
                value={field.value ?? ""}
                onValueChange={field.onChange}
              >
                <SelectTrigger
                  id="originatingSubmissionId"
                  className="w-full"
                >
                  <SelectValue placeholder="Select a regulatory activity" />
                </SelectTrigger>

                <SelectContent>
                  {(continuable ?? []).map((origin) => (
                    // Listed in business language, not as a bare sequence
                    // number: a filer chooses between activities, and the
                    // number is how eCTD identifies one, not how a person does.
                    <SelectItem key={origin.id} value={origin.id}>
                      {`${origin.submissionTypeName} — opened by ${String(
                        origin.sequenceNumber
                      ).padStart(4, "0")}`}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>

              <FieldError errors={[errors.originatingSubmissionId]} />
            </Field>
          )}
        />
      )}

      <Controller
        control={control}
        name="submissionSubTypeId"
        render={({ field }) => (
          <Field data-invalid={!!errors.submissionSubTypeId}>
            <FieldLabel htmlFor="submissionSubTypeId">
              What does this sequence do?
            </FieldLabel>

            <Select value={field.value ?? ""} onValueChange={field.onChange}>
              <SelectTrigger id="submissionSubTypeId" className="w-full">
                <SelectValue placeholder="Select what this sequence does" />
              </SelectTrigger>

              <SelectContent>
                {(subTypes ?? []).map((subType) => (
                  <SelectItem key={subType.id} value={subType.id}>
                    {subType.name}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>

            {/*
              Asked even when it looks obvious. The tempting shortcut — an
              opener is an Application, a continuer is an Amendment — is
              falsified by FDA's own example #23, an opening sequence whose
              sub-type is Report. RegOS records the filer's intent instead of
              inferring it.
            */}
            <p className="text-sm text-muted-foreground">
              This is not inferred from the choice above — an opening sequence
              can be a report as easily as an application.
            </p>

            <FieldError errors={[errors.submissionSubTypeId]} />
          </Field>
        )}
      />
    </>
  );
}
