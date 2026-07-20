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

import { useUpdateUserProfile } from "../hooks/useUpdateUserProfile";
import type { UserDetails } from "../types/UserDetails";
import {
  updateUserProfileSchema,
  type UpdateUserProfileFormValues,
} from "../validation/updateUserProfileSchema";

interface EditUserProfileFormProps {
  user: UserDetails;
  onSuccess(): void;
  onCancel(): void;
}

export function EditUserProfileForm({
  user,
  onSuccess,
  onCancel,
}: EditUserProfileFormProps) {
  const mutation = useUpdateUserProfile(user.id);

  const {
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<UpdateUserProfileFormValues>({
    resolver: zodResolver(updateUserProfileSchema),
    defaultValues: {
      firstName: user.firstName,
      lastName: user.lastName,
      email: user.email,
    },
  });

  async function onSubmit(values: UpdateUserProfileFormValues) {
    await mutation.mutateAsync(values);

    onSuccess();
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
      <FieldGroup>
        <Controller
          control={control}
          name="firstName"
          render={({ field }) => (
            <Field data-invalid={!!errors.firstName}>
              <FieldLabel htmlFor="firstName">First Name</FieldLabel>

              <Input id="firstName" {...field} />

              <FieldError errors={[errors.firstName]} />
            </Field>
          )}
        />

        <Controller
          control={control}
          name="lastName"
          render={({ field }) => (
            <Field data-invalid={!!errors.lastName}>
              <FieldLabel htmlFor="lastName">Last Name</FieldLabel>

              <Input id="lastName" {...field} />

              <FieldError errors={[errors.lastName]} />
            </Field>
          )}
        />

        <Controller
          control={control}
          name="email"
          render={({ field }) => (
            <Field data-invalid={!!errors.email}>
              <FieldLabel htmlFor="email">Email</FieldLabel>

              <Input id="email" type="email" {...field} />

              <FieldError errors={[errors.email]} />
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
        <Button
          type="button"
          variant="outline"
          onClick={onCancel}
          disabled={mutation.isPending}
        >
          Cancel
        </Button>

        {/* Disabled in flight so the profile cannot be submitted twice. */}
        <Button type="submit" disabled={mutation.isPending}>
          {mutation.isPending ? "Saving..." : "Save"}
        </Button>
      </div>
    </form>
  );
}
