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

import { useAddOrganizationIdentifier } from "../hooks/useAddOrganizationIdentifier";
import { useIdentifierSchemes } from "../hooks/useIdentifierSchemes";
import {
  addOrganizationIdentifierSchema,
  type AddOrganizationIdentifierFormValues,
} from "../validation/addOrganizationIdentifierSchema";

interface AddOrganizationIdentifierFormProps {
  organizationId: string;
  onSuccess(): void;
}

export function AddOrganizationIdentifierForm({
  organizationId,
  onSuccess,
}: AddOrganizationIdentifierFormProps) {
  const mutation = useAddOrganizationIdentifier(organizationId);
  const { data: schemes, isPending } = useIdentifierSchemes();

  const {
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<AddOrganizationIdentifierFormValues>({
    resolver: zodResolver(addOrganizationIdentifierSchema),
    defaultValues: { schemeId: "", value: "" },
  });

  async function onSubmit(values: AddOrganizationIdentifierFormValues) {
    try {
      await mutation.mutateAsync(values);
    } catch {
      // A refusal is an outcome, not a crash — the server's reason is rendered
      // from mutation.error below. Without this the rejection escapes
      // handleSubmit and reaches the window as an unhandled page error.
      return;
    }

    onSuccess();
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
      <FieldGroup>
        <Controller
          control={control}
          name="schemeId"
          render={({ field }) => (
            <Field data-invalid={!!errors.schemeId}>
              <FieldLabel htmlFor="schemeId">Scheme</FieldLabel>

              {/* Every scheme is offered, including ones already recorded: the
                  server states the one-per-scheme rule, and hiding the option
                  would be the UI restating it. */}
              <Select
                value={field.value}
                onValueChange={field.onChange}
                disabled={isPending}
              >
                <SelectTrigger id="schemeId" className="w-full">
                  <SelectValue
                    placeholder={
                      isPending ? "Loading schemes..." : "Select a scheme"
                    }
                  />
                </SelectTrigger>

                <SelectContent>
                  {(schemes ?? []).map((scheme) => (
                    <SelectItem key={scheme.id} value={scheme.id}>
                      {scheme.code} — {scheme.issuer}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>

              <FieldError errors={[errors.schemeId]} />
            </Field>
          )}
        />

        {/* "Identifier Value", not "Identifier": the dialog is already called
            Record Identifier, and the domain's word for this half of the
            scheme+value pair is Value. */}
        <Controller
          control={control}
          name="value"
          render={({ field }) => (
            <Field data-invalid={!!errors.value}>
              <FieldLabel htmlFor="value">Identifier Value</FieldLabel>

              <Input id="value" placeholder="150483782" {...field} />

              <FieldError errors={[errors.value]} />
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
          {mutation.isPending ? "Saving..." : "Record Identifier"}
        </Button>
      </div>
    </form>
  );
}
