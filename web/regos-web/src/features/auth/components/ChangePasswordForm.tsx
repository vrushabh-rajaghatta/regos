import { zodResolver } from "@hookform/resolvers/zod";
import { useQueryClient } from "@tanstack/react-query";
import { Controller, useForm } from "react-hook-form";
import { useNavigate } from "react-router-dom";

import { Button } from "@/components/ui/button";
import {
  Field,
  FieldError,
  FieldGroup,
  FieldLabel,
} from "@/components/ui/field";
import { Input } from "@/components/ui/input";

import { useChangePassword } from "../hooks/useChangePassword";
import {
  changePasswordSchema,
  type ChangePasswordFormValues,
} from "../validation/changePasswordSchema";

export function ChangePasswordForm() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const mutation = useChangePassword();

  const {
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<ChangePasswordFormValues>({
    resolver: zodResolver(changePasswordSchema),
    defaultValues: {
      currentPassword: "",
      newPassword: "",
      confirmPassword: "",
    },
  });

  // `mutate`, not `mutateAsync`: a wrong current password is an expected
  // outcome, and awaiting it would surface every one as an unhandled rejection
  // beside the message the user is meant to read.
  function onSubmit(values: ChangePasswordFormValues) {
    mutation.mutate(
      {
        currentPassword: values.currentPassword,
        newPassword: values.newPassword,
      },
      {
        onSuccess: () => {
          // The server has just ended every session, including this one, and
          // cleared both cookies (ADR-028). Staying on this page would show a
          // shell backed by cached data belonging to a session that no longer
          // exists, so the cache goes and so does the user.
          queryClient.clear();

          navigate("/login", { replace: true });
        },
      },
    );
  }

  return (
    <form
      onSubmit={handleSubmit(onSubmit)}
      className="space-y-6 max-w-sm"
      data-testid="change-password-form"
    >
      <FieldGroup>
        <Controller
          control={control}
          name="currentPassword"
          render={({ field }) => (
            <Field data-invalid={!!errors.currentPassword}>
              <FieldLabel htmlFor="currentPassword">
                Current Password
              </FieldLabel>

              <Input
                id="currentPassword"
                type="password"
                autoComplete="current-password"
                {...field}
              />

              <FieldError errors={[errors.currentPassword]} />
            </Field>
          )}
        />

        <Controller
          control={control}
          name="newPassword"
          render={({ field }) => (
            <Field data-invalid={!!errors.newPassword}>
              <FieldLabel htmlFor="newPassword">New Password</FieldLabel>

              <Input
                id="newPassword"
                type="password"
                autoComplete="new-password"
                {...field}
              />

              <FieldError errors={[errors.newPassword]} />
            </Field>
          )}
        />

        <Controller
          control={control}
          name="confirmPassword"
          render={({ field }) => (
            <Field data-invalid={!!errors.confirmPassword}>
              <FieldLabel htmlFor="confirmPassword">
                Confirm New Password
              </FieldLabel>

              <Input
                id="confirmPassword"
                type="password"
                autoComplete="new-password"
                {...field}
              />

              <FieldError errors={[errors.confirmPassword]} />
            </Field>
          )}
        />
      </FieldGroup>

      {mutation.isError && (
        <p className="text-sm text-destructive" role="alert">
          {mutation.error.message}
        </p>
      )}

      <p className="text-sm text-muted-foreground">
        Changing your password signs you out everywhere, including here.
      </p>

      <Button type="submit" disabled={mutation.isPending}>
        {mutation.isPending ? "Changing..." : "Change Password"}
      </Button>
    </form>
  );
}
