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

import { useCreateOrganization } from "../hooks/useCreateOrganization";
import { ORGANIZATION_TYPES } from "../types/OrganizationType";
import {
  createOrganizationSchema,
  type CreateOrganizationFormValues,
} from "../validation/createOrganizationSchema";

interface CreateOrganizationFormProps {
  onSuccess(): void;
}

export function CreateOrganizationForm({
  onSuccess,
}: CreateOrganizationFormProps) {
  const mutation = useCreateOrganization();

  // Only the two fields the API accepts. Status is set by the domain, and
  // there is no code or settings field until the backend has one.
  const {
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<CreateOrganizationFormValues>({
    resolver: zodResolver(createOrganizationSchema),
    defaultValues: {
      legalName: "",
      type: "",
    },
  });

  async function onSubmit(values: CreateOrganizationFormValues) {
    await mutation.mutateAsync(values);

    reset();

    onSuccess();
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
      <FieldGroup>
        <Controller
          control={control}
          name="legalName"
          render={({ field }) => (
            <Field data-invalid={!!errors.legalName}>
              <FieldLabel htmlFor="legalName">Legal Name</FieldLabel>

              <Input
                id="legalName"
                placeholder="Enter the registered legal name"
                {...field}
              />

              <FieldError errors={[errors.legalName]} />
            </Field>
          )}
        />

        <Controller
          control={control}
          name="type"
          render={({ field }) => (
            <Field data-invalid={!!errors.type}>
              <FieldLabel htmlFor="type">Organization Type</FieldLabel>

              <Select value={field.value} onValueChange={field.onChange}>
                <SelectTrigger id="type" className="w-full">
                  <SelectValue placeholder="Select a type" />
                </SelectTrigger>

                <SelectContent>
                  {ORGANIZATION_TYPES.map((type) => (
                    <SelectItem key={type.value} value={type.value}>
                      {type.label}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>

              <FieldError errors={[errors.type]} />
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
          {mutation.isPending ? "Creating..." : "Create Organization"}
        </Button>
      </div>
    </form>
  );
}
