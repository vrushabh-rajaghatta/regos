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

import { useLogin } from "../hooks/useLogin";
import { loginSchema, type LoginFormValues } from "../validation/loginSchema";

interface LoginFormProps {
  onSuccess(): void;
}

export function LoginForm({ onSuccess }: LoginFormProps) {
  const mutation = useLogin();

  const {
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginFormValues>({
    resolver: zodResolver(loginSchema),
    defaultValues: {
      email: "",
      password: "",
    },
  });

  // `mutate`, not `mutateAsync`: a rejected sign-in is an expected outcome, and
  // awaiting it here would surface every wrong password as an unhandled promise
  // rejection in the console alongside the message the user is meant to read.
  function onSubmit(values: LoginFormValues) {
    mutation.mutate(values, { onSuccess });
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

        <Controller
          control={control}
          name="password"
          render={({ field }) => (
            <Field data-invalid={!!errors.password}>
              <FieldLabel htmlFor="password">Password</FieldLabel>

              <Input
                id="password"
                type="password"
                autoComplete="current-password"
                {...field}
              />

              <FieldError errors={[errors.password]} />
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
        {mutation.isPending ? "Signing in..." : "Sign In"}
      </Button>

      <p className="text-sm text-muted-foreground text-center">
        <Link to="/forgot-password" className="underline">
          Forgot password?
        </Link>
      </p>
    </form>
  );
}
