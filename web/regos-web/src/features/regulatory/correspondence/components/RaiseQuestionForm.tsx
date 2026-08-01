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

import { useRaiseQuestion } from "../hooks/useRaiseQuestion";
import {
  raiseQuestionSchema,
  type RaiseQuestionFormValues,
} from "../validation/raiseQuestionSchema";

interface RaiseQuestionFormProps {
  correspondenceId: string;
  onSuccess(): void;
}

export function RaiseQuestionForm({
  correspondenceId,
  onSuccess,
}: RaiseQuestionFormProps) {
  const mutation = useRaiseQuestion(correspondenceId);

  const {
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<RaiseQuestionFormValues>({
    resolver: zodResolver(raiseQuestionSchema),
    defaultValues: { number: "", text: "", targetResponseOn: "" },
  });

  async function onSubmit(values: RaiseQuestionFormValues) {
    try {
      await mutation.mutateAsync({
        number: values.number,
        text: values.text,
        targetResponseOn: values.targetResponseOn || null,
      });
    } catch {
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
          name="number"
          render={({ field }) => (
            <Field data-invalid={!!errors.number}>
              <FieldLabel htmlFor="questionNumber">
                Number, as the letter gives it
              </FieldLabel>

              <Input id="questionNumber" {...field} />

              {errors.number && <FieldError>{errors.number.message}</FieldError>}
            </Field>
          )}
        />

        <Controller
          control={control}
          name="text"
          render={({ field }) => (
            <Field data-invalid={!!errors.text}>
              <FieldLabel htmlFor="questionText">What they asked</FieldLabel>

              <textarea
                id="questionText"
                rows={4}
                className="w-full rounded-md border bg-transparent p-3 text-sm"
                {...field}
              />

              {errors.text && <FieldError>{errors.text.message}</FieldError>}
            </Field>
          )}
        />

        <Controller
          control={control}
          name="targetResponseOn"
          render={({ field }) => (
            <Field data-invalid={!!errors.targetResponseOn}>
              {/* "Target", not "Due": the letter carries the regulator's
                  deadline and this is our internal plan. Both appear in the
                  due view, so they never share a word. */}
              <FieldLabel htmlFor="questionTargetResponseOn">
                Our target response date (optional)
              </FieldLabel>

              <Input
                id="questionTargetResponseOn"
                type="date"
                {...field}
              />

              {errors.targetResponseOn && (
                <FieldError>{errors.targetResponseOn.message}</FieldError>
              )}
            </Field>
          )}
        />
      </FieldGroup>

      {mutation.isError && (
        <p className="text-sm text-destructive" data-testid="raise-question-error">
          {(mutation.error as Error).message}
        </p>
      )}

      <div className="flex justify-end">
        {/* "Raise", not "Raise question": the section's own action carries the
            full name (docs/engineering/accessible-names.md). */}
        <Button type="submit" disabled={mutation.isPending}>
          {mutation.isPending ? "Raising..." : "Raise"}
        </Button>
      </div>
    </form>
  );
}
