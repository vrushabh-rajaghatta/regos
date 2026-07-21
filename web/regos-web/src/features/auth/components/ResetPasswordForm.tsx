import { zodResolver } from "@hookform/resolvers/zod";
import { Controller, useForm } from "react-hook-form";
import { Link, useNavigate } from "react-router-dom";

import { Button } from "@/components/ui/button";
import {
  Field,
  FieldError,
  FieldGroup,
  FieldLabel,
} from "@/components/ui/field";
import { Input } from "@/components/ui/input";

import { useCompletePasswordReset } from "../hooks/usePasswordReset";
import {
  resetPasswordSchema,
  type ResetPasswordFormValues,
} from "../validation/passwordResetSchemas";

interface ResetPasswordFormProps {
  token: string;
}

export function ResetPasswordForm({ token }: ResetPasswordFormProps) {
  const navigate = useNavigate();
  const mutation = useCompletePasswordReset();

  const {
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<ResetPasswordFormValues>({
    resolver: zodResolver(resetPasswordSchema),
    defaultValues: { password: "", confirmPassword: "" },
  });

  // `mutate`, not `mutateAsync`: a dead link is an expected outcome.
  function onSubmit(values: ResetPasswordFormValues) {
    mutation.mutate(
      { token, password: values.password },
      {
        // Resetting does not sign you in - it proves you can read a mailbox,
        // not that you know the password you just chose. Sessions come from one
        // place, and the user goes there next.
        onSuccess: () => navigate("/login", { replace: true }),
      },
    );
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
      <FieldGroup>
        <Controller
          control={control}
          name="password"
          render={({ field }) => (
            <Field data-invalid={!!errors.password}>
              <FieldLabel htmlFor="password">New Password</FieldLabel>

              <Input
                id="password"
                type="password"
                autoComplete="new-password"
                {...field}
              />

              <FieldError errors={[errors.password]} />
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
        <div className="space-y-2">
          <p className="text-sm text-destructive" role="alert">
            {mutation.error.message}
          </p>

          <p className="text-sm text-muted-foreground">
            <Link to="/forgot-password" className="underline">
              Request a new link
            </Link>
          </p>
        </div>
      )}

      <Button type="submit" className="w-full" disabled={mutation.isPending}>
        {mutation.isPending ? "Resetting..." : "Reset Password"}
      </Button>
    </form>
  );
}
