import { useMemo } from "react";
import { useNavigate } from "react-router-dom";
import { zodResolver } from "@hookform/resolvers/zod";
import { Controller, useForm } from "react-hook-form";

import { Button } from "@/components/ui/button";
import {
  Field,
  FieldError,
  FieldGroup,
  FieldLabel,
} from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";

import {
  FORMAT_DESCRIPTIONS,
  SUBMISSION_FORMATS,
  formatLabel,
} from "../utils/formatLabel";
import { useCreateSubmission } from "../hooks/useCreateSubmission";
import { RegulatoryActivityField } from "./RegulatoryActivityField";
import {
  createSubmissionSchema,
  type CreateSubmissionFormValues,
} from "../validation/createSubmissionSchema";

interface Props {
  globalProductId: string;
  applicationId: string;
  /** The application's authority — the activity vocabulary is scoped to it. */
  authorityId: string;
  onSuccess: () => void;
}

export function CreateSubmissionForm({
  globalProductId,
  applicationId,
  authorityId,
  onSuccess,
}: Props) {
  const navigate = useNavigate();

  // No application-type picker: the type belongs to the application, and every
  // sequence filed under it inherits the classification (EPIC-007a S001).
  const mutation = useCreateSubmission(applicationId);

  const {
    control,
    handleSubmit,
    reset,
    watch,
    formState: { errors },
  } = useForm<CreateSubmissionFormValues>({
    resolver: zodResolver(createSubmissionSchema),
    defaultValues: {
      title: "",
      // Shown selected rather than assumed silently: eCTD is the only format
      // an FDA IND accepts today, and the user can see and change it.
      format: "Ectd",
      // Most sequences open something; the alternative is one click away and
      // is disabled until a published filing exists to continue.
      activityChoice: "start",
      submissionTypeId: "",
      originatingSubmissionId: "",
      // No default: unlike format, no value here is the obvious one.
      submissionSubTypeId: "",
    },
  });

  const activityChoice = watch("activityChoice");

  const formatItems = useMemo(
    () =>
      Object.fromEntries(
        SUBMISSION_FORMATS.map((format) => [format, formatLabel(format)])
      ),
    []
  );

  async function onSubmit(values: CreateSubmissionFormValues) {
    const { id } = await mutation.mutateAsync({
      title: values.title,
      format: values.format,
      submissionSubTypeId: values.submissionSubTypeId,
      // Exactly one of these is sent. The schema has already refused the
      // other combinations, and the domain type could not represent them.
      submissionTypeId:
        values.activityChoice === "start"
          ? values.submissionTypeId
          : undefined,
      originatingSubmissionId:
        values.activityChoice === "continue"
          ? values.originatingSubmissionId
          : undefined,
    });

    reset();
    onSuccess();

    // Standard creation flow: take the user straight into the workspace of
    // the entity they just created (product -> application -> submission).
    navigate(
      `/regulatory/products/${globalProductId}/applications/${applicationId}/submissions/${id}`
    );
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
      <FieldGroup>
        <Controller
          control={control}
          name="title"
          render={({ field }) => (
            <Field data-invalid={!!errors.title}>
              <FieldLabel htmlFor="title">Submission Title</FieldLabel>

              <Input id="title" placeholder="e.g. Initial 510(k)" {...field} />

              <FieldError errors={[errors.title]} />
            </Field>
          )}
        />

        <Controller
          control={control}
          name="format"
          render={({ field }) => (
            <Field data-invalid={!!errors.format}>
              <FieldLabel htmlFor="format">Submission Format</FieldLabel>

              <Select
                items={formatItems}
                value={field.value}
                onValueChange={field.onChange}
              >
                <SelectTrigger id="format" className="w-full">
                  <SelectValue placeholder="Select submission format" />
                </SelectTrigger>

                <SelectContent>
                  {SUBMISSION_FORMATS.map((format) => (
                    <SelectItem key={format} value={format}>
                      {formatLabel(format)}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>

              <p className="text-sm text-muted-foreground">
                {FORMAT_DESCRIPTIONS[field.value]}
              </p>

              <FieldError errors={[errors.format]} />
            </Field>
          )}
        />

        <RegulatoryActivityField
          control={control}
          errors={errors}
          applicationId={applicationId}
          authorityId={authorityId}
          activityChoice={activityChoice}
        />
      </FieldGroup>

      {mutation.isError && (
        <p className="text-sm font-normal text-destructive">
          {mutation.error.message}
        </p>
      )}

      <div className="flex justify-end gap-2">
        <Button type="submit" disabled={mutation.isPending}>
          {mutation.isPending ? "Creating..." : "Create Submission"}
        </Button>
      </div>
    </form>
  );
}
