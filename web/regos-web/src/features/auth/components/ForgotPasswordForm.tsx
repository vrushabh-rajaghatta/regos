import { zodResolver } from "@hookform/resolvers/zod";
import { Controller, useForm } from "react-hook-form";
import { Link } from "react-router-dom";

import { Button } from "@/components/ui/button";
import {
  Field,
  FieldError,
  FieldGroup,
  FieldLabel,
} from "@/components/ui/field";
import { Input } from "@/components/ui/input";

import { useRequestPasswordReset } from "../hooks/usePasswordReset";
import {
  forgotPasswordSchema,
  type ForgotPasswordFormValues,
} from "../validation/passwordResetSchemas";

export function ForgotPasswordForm() {
  const mutation = useRequestPasswordReset();

  const {
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<ForgotPasswordFormValues>({
    resolver: zodResolver(forgotPasswordSchema),
    defaultValues: { email: "" },
  });

  // The confirmation, shown for every address that was submitted successfully.
  // It says "if" on purpose: the API cannot tell the browser whether an account
  // exists, and this screen must not appear to know either. Anything more
  // specific here would hand back the enumeration oracle the API withheld.
  if (mutation.isSuccess) {
    return (
      <div className="space-y-6" data-testid="password-reset-requested">
        <p className="text-sm" role="status">
          If that email address belongs to an active RegOS account, a link to
          choose a new password is on its way. The link expires in one hour.
        </p>

        <p className="text-sm text-muted-foreground">
          <Link to="/login" className="underline">
            Back to sign in
          </Link>
        </p>
      </div>
    );
  }

  // `mutate`, not `mutateAsync`: the same reasoning as sign-in - an awaited
  // rejection would surface in the console beside the message the user reads.
  function onSubmit(values: ForgotPasswordFormValues) {
    mutation.mutate(values);
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
      <FieldGroup>
        <Controller
          control={control}
          name="email"
          render={({ field }) => (
            <Field data-invalid={!!errors.email}>
              <FieldLabel htmlFor="email">Email Address</FieldLabel>

              <Input
                id="email"
                type="email"
                autoComplete="username"
                placeholder="you@example.com"
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

      <Button type="submit" className="w-full" disabled={mutation.isPending}>
        {mutation.isPending ? "Sending..." : "Send Reset Link"}
      </Button>

      <p className="text-sm text-muted-foreground">
        Remembered it?{" "}
        <Link to="/login" className="underline">
          Sign in
        </Link>
      </p>
    </form>
  );
}
