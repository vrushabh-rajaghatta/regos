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

import { useUpdateOrganization } from "../hooks/useUpdateOrganization";
import type { OrganizationDetails } from "../types/OrganizationDetails";
import { ORGANIZATION_TYPES } from "../types/OrganizationType";
import {
  updateOrganizationSchema,
  type UpdateOrganizationFormValues,
} from "../validation/updateOrganizationSchema";

interface EditOrganizationFormProps {
  organization: OrganizationDetails;
  onSuccess(): void;
}

export function EditOrganizationForm({
  organization,
  onSuccess,
}: EditOrganizationFormProps) {
  const mutation = useUpdateOrganization(organization.id);

  // Status is absent by design: it belongs to Activate and Deactivate, not to
  // an edit. Submitting unchanged values is a no-op, not an error.
  const {
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<UpdateOrganizationFormValues>({
    resolver: zodResolver(updateOrganizationSchema),
    defaultValues: {
      legalName: organization.legalName,
      type: organization.type,
      acronym: organization.acronym ?? "",
      nameNativeLanguage: organization.nameNativeLanguage ?? "",
    },
  });

  async function onSubmit(values: UpdateOrganizationFormValues) {
    // Empty means "not recorded", not "the empty string" — the server treats a
    // null and a blank identically, and this keeps the round trip honest.
    await mutation.mutateAsync({
      ...values,
      acronym: values.acronym || null,
      nameNativeLanguage: values.nameNativeLanguage || null,
    });

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

              <Input id="legalName" {...field} />

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

        <Controller
          control={control}
          name="acronym"
          render={({ field }) => (
            <Field data-invalid={!!errors.acronym}>
              <FieldLabel htmlFor="acronym">Acronym</FieldLabel>

              <Input id="acronym" placeholder="DML" {...field} />

              <FieldError errors={[errors.acronym]} />
            </Field>
          )}
        />

        <Controller
          control={control}
          name="nameNativeLanguage"
          render={({ field }) => (
            <Field data-invalid={!!errors.nameNativeLanguage}>
              <FieldLabel htmlFor="nameNativeLanguage">
                Name (Native Language)
              </FieldLabel>

              <Input
                id="nameNativeLanguage"
                placeholder="デモ製薬株式会社"
                {...field}
              />

              <FieldError errors={[errors.nameNativeLanguage]} />
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
          {mutation.isPending ? "Saving..." : "Save Changes"}
        </Button>
      </div>
    </form>
  );
}
