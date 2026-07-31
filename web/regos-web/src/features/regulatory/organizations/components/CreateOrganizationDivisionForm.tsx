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

import { useCreateOrganizationDivision } from "../hooks/useCreateOrganizationDivision";
import { today } from "../utils/today";
import {
  createOrganizationDivisionSchema,
  type CreateOrganizationDivisionFormValues,
} from "../validation/createOrganizationDivisionSchema";

interface CreateOrganizationDivisionFormProps {
  organizationId: string;
  onSuccess(): void;
}

export function CreateOrganizationDivisionForm({
  organizationId,
  onSuccess,
}: CreateOrganizationDivisionFormProps) {
  const mutation = useCreateOrganizationDivision(organizationId);

  const {
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<CreateOrganizationDivisionFormValues>({
    resolver: zodResolver(createOrganizationDivisionSchema),
    defaultValues: { name: "", statusDate: today(), acronym: "" },
  });

  async function onSubmit(values: CreateOrganizationDivisionFormValues) {
    await mutation.mutateAsync({
      name: values.name,
      statusDate: values.statusDate,
      acronym: values.acronym || null,
    });

    onSuccess();
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
      <FieldGroup>
        <Controller
          control={control}
          name="name"
          render={({ field }) => (
            <Field data-invalid={!!errors.name}>
              <FieldLabel htmlFor="divisionName">Name</FieldLabel>

              <Input
                id="divisionName"
                placeholder="Regulatory Affairs"
                {...field}
              />

              <FieldError errors={[errors.name]} />
            </Field>
          )}
        />

        <Controller
          control={control}
          name="acronym"
          render={({ field }) => (
            <Field data-invalid={!!errors.acronym}>
              <FieldLabel htmlFor="divisionAcronym">Acronym</FieldLabel>

              <Input id="divisionAcronym" placeholder="RA" {...field} />

              <FieldError errors={[errors.acronym]} />
            </Field>
          )}
        />

        <Controller
          control={control}
          name="statusDate"
          render={({ field }) => (
            <Field data-invalid={!!errors.statusDate}>
              <FieldLabel htmlFor="divisionStatusDate">Established</FieldLabel>

              <Input id="divisionStatusDate" type="date" {...field} />

              <FieldError errors={[errors.statusDate]} />
            </Field>
          )}
        />
      </FieldGroup>

      {mutation.isError && (
        <p className="text-sm text-destructive" role="alert">
          {mutation.error.message}
        </p>
      )}

      <div className="flex justify-end gap-2">
        <Button type="submit" disabled={mutation.isPending}>
          {mutation.isPending ? "Saving..." : "Add Division"}
        </Button>
      </div>
    </form>
  );
}
