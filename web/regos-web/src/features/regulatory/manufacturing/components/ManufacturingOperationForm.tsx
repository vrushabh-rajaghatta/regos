import { zodResolver } from "@hookform/resolvers/zod";
import { Controller, useForm } from "react-hook-form";

import { Button } from "@/components/ui/button";
import { Field, FieldError, FieldLabel } from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { useSiteDirectory } from "@/features/regulatory/organizations/hooks/useSiteDirectory";

import { useManufacturingVocabulary } from "../hooks/useManufacturingVocabulary";
import { useRecordManufacturingOperation } from "../hooks/useRecordManufacturingOperation";
import {
  manufacturingOperationSchema,
  type ManufacturingOperationFormValues,
} from "../validation/manufacturingOperationSchema";

interface ManufacturingOperationFormProps {
  medicinalProductId: string;
  onSuccess(): void;
}

/**
 * Which site does what for this market, and since when.
 *
 * **The site list is the tenant-wide directory, unfiltered by type.** It would
 * be easy to offer only `Manufacturing` sites, and it would be wrong: a testing
 * laboratory performs QC testing and a warehouse performs importation. What a
 * site *is* and what it *does for this product* are different facts, and only
 * the second one belongs here.
 */
export function ManufacturingOperationForm({
  medicinalProductId,
  onSuccess,
}: ManufacturingOperationFormProps) {
  const { data: vocabulary } = useManufacturingVocabulary();
  const { data: sites } = useSiteDirectory();

  const mutation = useRecordManufacturingOperation(medicinalProductId);

  const {
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<ManufacturingOperationFormValues>({
    resolver: zodResolver(manufacturingOperationSchema),
    defaultValues: {
      organizationSiteId: "",
      operationCode: "",
      effectiveFrom: "",
    },
  });

  async function onSubmit(values: ManufacturingOperationFormValues) {
    try {
      await mutation.mutateAsync(values);
    } catch {
      // A refusal is an outcome, not a crash — the server's reason renders
      // below and the form keeps what was chosen.
      return;
    }

    onSuccess();
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      <Controller
        control={control}
        name="organizationSiteId"
        render={({ field }) => (
          <Field data-invalid={!!errors.organizationSiteId}>
            <FieldLabel htmlFor="operation-site">Site</FieldLabel>

            <select
              id="operation-site"
              className="h-9 w-full rounded-md border bg-transparent px-3 text-sm"
              {...field}
            >
              <option value="">Choose a site</option>

              {(sites ?? []).map((site) => (
                <option key={site.siteId} value={site.siteId}>
                  {site.name} — {site.countryName}
                </option>
              ))}
            </select>

            {errors.organizationSiteId && (
              <FieldError>{errors.organizationSiteId.message}</FieldError>
            )}

            {/* Why the list is not narrowed to manufacturing sites. */}
            <p className="text-xs text-muted-foreground">
              Every site in the registry — a laboratory tests, a warehouse
              imports. What a site is and what it does here are two facts.
            </p>
          </Field>
        )}
      />

      <Controller
        control={control}
        name="operationCode"
        render={({ field }) => (
          <Field data-invalid={!!errors.operationCode}>
            <FieldLabel htmlFor="operation-code">Operation</FieldLabel>

            <select
              id="operation-code"
              className="h-9 w-full rounded-md border bg-transparent px-3 text-sm"
              {...field}
            >
              <option value="">Choose an operation</option>

              {(vocabulary?.operations ?? []).map((concept) => (
                <option key={concept.code} value={concept.code}>
                  {concept.display}
                </option>
              ))}
            </select>

            {errors.operationCode && (
              <FieldError>{errors.operationCode.message}</FieldError>
            )}
          </Field>
        )}
      />

      <Controller
        control={control}
        name="effectiveFrom"
        render={({ field }) => (
          <Field data-invalid={!!errors.effectiveFrom}>
            <FieldLabel htmlFor="operation-from">Performing since</FieldLabel>

            <Input id="operation-from" type="date" {...field} />

            {errors.effectiveFrom && (
              <FieldError>{errors.effectiveFrom.message}</FieldError>
            )}

            {/* Asked rather than defaulted to today, for the reason a pack
                authorisation's date is asked for: it is routinely in the past. */}
            <p className="text-xs text-muted-foreground">
              The business date, not today's — an operation recorded now may
              have run since 2019.
            </p>
          </Field>
        )}
      />

      {mutation.isError && (
        <p
          className="text-sm text-destructive"
          data-testid="manufacturing-error"
        >
          {(mutation.error as Error).message}
        </p>
      )}

      <div className="flex justify-end">
        <Button type="submit" disabled={mutation.isPending}>
          {mutation.isPending ? "Saving..." : "Record operation"}
        </Button>
      </div>
    </form>
  );
}
