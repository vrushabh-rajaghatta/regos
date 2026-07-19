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

import { useSubmissionTypes } from "../hooks/useSubmissionTypes";
import { useCreateSubmission } from "../hooks/useCreateSubmission";
import {
  createSubmissionSchema,
  type CreateSubmissionFormValues,
} from "../validation/createSubmissionSchema";

interface Props {
  productId: string;
  applicationId: string;
  authorityId: string;
  onSuccess: () => void;
}

export function CreateSubmissionForm({
  productId,
  applicationId,
  authorityId,
  onSuccess,
}: Props) {
  const navigate = useNavigate();

  // Types are scoped to the application's authority, so the user can only
  // choose one the backend will accept (Rule 3).
  const submissionTypesQuery = useSubmissionTypes(authorityId);

  const mutation = useCreateSubmission(applicationId);

  const {
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<CreateSubmissionFormValues>({
    resolver: zodResolver(createSubmissionSchema),
    defaultValues: {
      title: "",
      submissionTypeId: "",
    },
  });

  // Value->label map so the Select trigger displays the type name rather
  // than the raw id.
  const submissionTypeItems = useMemo(
    () =>
      Object.fromEntries(
        (submissionTypesQuery.data ?? []).map((type) => [type.id, type.name])
      ),
    [submissionTypesQuery.data]
  );

  async function onSubmit(values: CreateSubmissionFormValues) {
    const { id } = await mutation.mutateAsync({
      title: values.title,
      submissionTypeId: values.submissionTypeId,
    });

    reset();
    onSuccess();

    // Standard creation flow: take the user straight into the workspace of
    // the entity they just created (product -> application -> submission).
    navigate(
      `/regulatory/products/${productId}/applications/${applicationId}/submissions/${id}`
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
          name="submissionTypeId"
          render={({ field }) => (
            <Field data-invalid={!!errors.submissionTypeId}>
              <FieldLabel htmlFor="submissionTypeId">
                Submission Type
              </FieldLabel>

              <Select
                items={submissionTypeItems}
                value={field.value}
                onValueChange={field.onChange}
              >
                <SelectTrigger id="submissionTypeId" className="w-full">
                  <SelectValue placeholder="Select submission type" />
                </SelectTrigger>

                <SelectContent>
                  {(submissionTypesQuery.data ?? []).map((type) => (
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
