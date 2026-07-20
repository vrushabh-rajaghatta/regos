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

import { PRODUCT_TYPES } from "../constants/productTypes";
import { useUpdateProduct } from "../hooks/useUpdateProduct";
import type { ProductDetails } from "../types/ProductDetails";
import {
  updateProductSchema,
  type UpdateProductFormValues,
} from "../validation/updateProductSchema";

interface EditProductFormProps {
  product: ProductDetails;
  onSuccess(): void;
  onCancel(): void;
}

export function EditProductForm({
  product,
  onSuccess,
  onCancel,
}: EditProductFormProps) {
  const mutation = useUpdateProduct(product.id);

  const {
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<UpdateProductFormValues>({
    resolver: zodResolver(updateProductSchema),
    defaultValues: {
      name: product.name,
      type: product.type as UpdateProductFormValues["type"],
    },
  });

  async function onSubmit(values: UpdateProductFormValues) {
    await mutation.mutateAsync(values);

    onSuccess();
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
      <FieldGroup>
        {/* The code is shown but not editable: it identifies the product
            within the organization and changing it is a separate capability. */}
        <Field>
          <FieldLabel htmlFor="code">Product Code</FieldLabel>

          <Input id="code" value={product.code} readOnly disabled />

          <p className="text-sm text-muted-foreground">
            A product code cannot be changed after registration.
          </p>
        </Field>

        <Controller
          control={control}
          name="name"
          render={({ field }) => (
            <Field data-invalid={!!errors.name}>
              <FieldLabel htmlFor="name">Product Name</FieldLabel>

              <Input id="name" {...field} />

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

              <Select
                value={field.value}
                onValueChange={(value) => field.onChange(value ?? field.value)}
              >
                <SelectTrigger id="type" aria-label="Product Type">
                  <SelectValue placeholder="Select a type" />
                </SelectTrigger>

                <SelectContent>
                  {PRODUCT_TYPES.map((option) => (
                    <SelectItem key={option.value} value={option.value}>
                      {option.label}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>

              <FieldError errors={[errors.type]} />
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
        <Button
          type="button"
          variant="outline"
          onClick={onCancel}
          disabled={mutation.isPending}
        >
          Cancel
        </Button>

        {/* Disabled in flight so the product cannot be submitted twice. */}
        <Button type="submit" disabled={mutation.isPending}>
          {mutation.isPending ? "Saving..." : "Save"}
        </Button>
      </div>
    </form>
  );
}
