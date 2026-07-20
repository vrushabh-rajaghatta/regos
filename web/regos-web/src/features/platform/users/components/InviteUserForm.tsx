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

import { useOrganizations } from "@/features/regulatory/masterData/hooks/useOrganizations";

import { useInviteUser } from "../hooks/useInviteUser";
import {
  inviteUserSchema,
  type InviteUserFormValues,
} from "../validation/inviteUserSchema";

interface InviteUserFormProps {
  onSuccess(): void;
}

export function InviteUserForm({ onSuccess }: InviteUserFormProps) {
  const mutation = useInviteUser();

  // No authenticated user context yet, so the organization is chosen explicitly.
  // Only active organizations can accept users (the API enforces this too).
  const { data: organizations } = useOrganizations();
  const activeOrganizations =
    organizations?.filter((organization) => organization.status === "Active") ??
    [];

  const {
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<InviteUserFormValues>({
    resolver: zodResolver(inviteUserSchema),
    defaultValues: {
      organizationId: "",
      firstName: "",
      lastName: "",
      email: "",
    },
  });

  async function onSubmit(values: InviteUserFormValues) {
    await mutation.mutateAsync(values);

    reset();

    onSuccess();
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
      <FieldGroup>
        <Controller
          control={control}
          name="organizationId"
          render={({ field }) => (
            <Field data-invalid={!!errors.organizationId}>
              <FieldLabel htmlFor="organizationId">Organization</FieldLabel>

              <Select onValueChange={field.onChange} value={field.value}>
                <SelectTrigger id="organizationId">
                  <SelectValue placeholder="Select an organization" />
                </SelectTrigger>

                <SelectContent>
                  {activeOrganizations.map((organization) => (
                    <SelectItem key={organization.id} value={organization.id}>
                      {organization.legalName}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>

              <FieldError errors={[errors.organizationId]} />
            </Field>
          )}
        />

        <Controller
          control={control}
          name="firstName"
          render={({ field }) => (
            <Field data-invalid={!!errors.firstName}>
              <FieldLabel htmlFor="firstName">First Name</FieldLabel>

              <Input
                id="firstName"
                placeholder="Enter first name"
                {...field}
              />

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

              <Input id="lastName" placeholder="Enter last name" {...field} />

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

              <Input
                id="email"
                type="email"
                placeholder="name@example.com"
                {...field}
              />

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
        <Button type="submit" disabled={mutation.isPending}>
          {mutation.isPending ? "Inviting..." : "Invite User"}
        </Button>
      </div>
    </form>
  );
}
