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

import { STUDY_KINDS } from "../constants/studyKinds";
import { useRegisterStudy } from "../hooks/useRegisterStudy";
import {
  registerStudySchema,
  type RegisterStudyFormValues,
} from "../validation/registerStudySchema";

interface RegisterStudyFormProps {
  onSuccess(): void;
}

/**
 * Two facts and which kind. **Study ID** is the screen's word for what the
 * domain calls the sponsor study identifier — RegOS records the code the
 * sponsor already uses, it does not issue one.
 */
export function RegisterStudyForm({ onSuccess }: RegisterStudyFormProps) {
  const mutation = useRegisterStudy();

  const {
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<RegisterStudyFormValues>({
    resolver: zodResolver(registerStudySchema),
    defaultValues: {
      // Non-clinical first: it is the half that blocks an IND today.
      kind: "NonClinical",
      sponsorStudyIdentifier: "",
      title: "",
    },
  });

  async function onSubmit(values: RegisterStudyFormValues) {
    try {
      await mutation.mutateAsync(values);
    } catch {
      // A refusal is an outcome, not a crash. The server's reason renders
      // below and the form keeps what was typed — a duplicate study ID is
      // usually one character away from being right.
      return;
    }

    reset();

    onSuccess();
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
      <FieldGroup>
        <Controller
          control={control}
          name="kind"
          render={({ field }) => (
            <Field data-invalid={!!errors.kind}>
              <FieldLabel htmlFor="kind">Kind</FieldLabel>

              <Select onValueChange={field.onChange} value={field.value}>
                <SelectTrigger id="kind">
                  <SelectValue placeholder="Select a kind" />
                </SelectTrigger>

                <SelectContent>
                  {STUDY_KINDS.map((kind) => (
                    <SelectItem key={kind.value} value={kind.value}>
                      {kind.label} — {kind.hint}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>

              <FieldError errors={[errors.kind]} />
            </Field>
          )}
        />

        <Controller
          control={control}
          name="sponsorStudyIdentifier"
          render={({ field }) => (
            <Field data-invalid={!!errors.sponsorStudyIdentifier}>
              <FieldLabel htmlFor="sponsorStudyIdentifier">Study ID</FieldLabel>

              <Input
                id="sponsorStudyIdentifier"
                placeholder="e.g. TOX-2024-001"
                {...field}
              />

              <p className="text-xs text-muted-foreground">
                The code your organisation already uses for this study. It must
                stay the same in every submission that reports it.
              </p>

              <FieldError errors={[errors.sponsorStudyIdentifier]} />
            </Field>
          )}
        />

        <Controller
          control={control}
          name="title"
          render={({ field }) => (
            <Field data-invalid={!!errors.title}>
              <FieldLabel htmlFor="title">Title</FieldLabel>

              <Input
                id="title"
                placeholder="The full title of the study"
                {...field}
              />

              <FieldError errors={[errors.title]} />
            </Field>
          )}
        />
      </FieldGroup>

      {mutation.isError && (
        <p className="text-sm text-destructive" data-testid="register-study-error">
          {(mutation.error as Error).message}
        </p>
      )}

      <div className="flex justify-end gap-2">
        <Button type="submit" disabled={mutation.isPending}>
          {mutation.isPending ? "Registering..." : "Register study"}
        </Button>
      </div>
    </form>
  );
}
