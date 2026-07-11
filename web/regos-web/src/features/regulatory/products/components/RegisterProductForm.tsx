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

import { useRegisterProduct } from "../hooks/useRegisterProduct";
import {
  registerProductSchema,
  type RegisterProductFormValues,
} from "../validation/registerProductSchema";

interface RegisterProductFormProps {
  onSuccess(): void;
}

export function RegisterProductForm({ onSuccess }: RegisterProductFormProps) {
  const mutation = useRegisterProduct();

  const {
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<RegisterProductFormValues>({
    resolver: zodResolver(registerProductSchema),
    defaultValues: {
      name: "",
      type: "Drug",
    },
  });

  async function onSubmit(values: RegisterProductFormValues) {
    await mutation.mutateAsync(values);

    reset();

    onSuccess();
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
      <FieldGroup>
        <Controller
          control={control}
          name="name"
          render={({ field }) => (
            <Field data-invalid={!!errors.name}>
              <FieldLabel htmlFor="name">Product Name</FieldLabel>

              <Input
                id="name"
                placeholder="Enter product name"
                {...field}
              />

              <FieldError errors={[errors.name]} />
            </Field>
          )}
        />

        <Controller
          control={control}
          name="type"
          render={({ field }) => (
            <Field data-invalid={!!errors.type}>
              <FieldLabel htmlFor="type">Product Type</FieldLabel>

              <Select onValueChange={field.onChange} value={field.value}>
                <SelectTrigger id="type">
                  <SelectValue placeholder="Select product type" />
                </SelectTrigger>

                <SelectContent>
                  <SelectItem value="MedicalDevice">Medical Device</SelectItem>

                  <SelectItem value="Drug">Drug</SelectItem>

                  <SelectItem value="Biologic">Biologic</SelectItem>
                </SelectContent>
              </Select>

              <FieldError errors={[errors.type]} />
            </Field>
          )}
        />
      </FieldGroup>

      <div className="flex justify-end gap-2">
        <Button type="submit" disabled={mutation.isPending}>
          {mutation.isPending ? "Registering..." : "Register Product"}
        </Button>
      </div>
    </form>
  );
}
