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

import { useAcceptInvitation } from "../hooks/useAcceptInvitation";
import {
  acceptInvitationSchema,
  type AcceptInvitationFormValues,
} from "../validation/acceptInvitationSchema";

interface AcceptInvitationFormProps {
  token: string;
}

export function AcceptInvitationForm({ token }: AcceptInvitationFormProps) {
  const navigate = useNavigate();
  const mutation = useAcceptInvitation();

  const {
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<AcceptInvitationFormValues>({
    resolver: zodResolver(acceptInvitationSchema),
    defaultValues: { password: "", confirmPassword: "" },
  });

  // `mutate`, not `mutateAsync`: a dead link is an expected outcome, and
  // awaiting it would surface every one as an unhandled rejection beside the
  // message the user is meant to read.
  function onSubmit(values: AcceptInvitationFormValues) {
    mutation.mutate(
      { token, password: values.password },
      {
        // Accepting does not sign you in - it proves you were invited, not that
        // you know the password you just chose. Sessions come from one place.
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
              <FieldLabel htmlFor="password">Password</FieldLabel>

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
                Confirm Password
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
            Already set your password?{" "}
            <Link to="/login" className="underline">
              Sign in
            </Link>
          </p>
        </div>
      )}

      <Button type="submit" className="w-full" disabled={mutation.isPending}>
        {mutation.isPending ? "Setting password..." : "Set Password"}
      </Button>
    </form>
  );
}
