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
import { useCountries } from "@/features/regulatory/masterData/hooks/useCountries";

import { useCreateOrganizationSite } from "../hooks/useCreateOrganizationSite";
import { ORGANIZATION_SITE_TYPES } from "../types/OrganizationSiteType";
import { today } from "../utils/today";
import {
  createOrganizationSiteSchema,
  type CreateOrganizationSiteFormValues,
} from "../validation/createOrganizationSiteSchema";

interface CreateOrganizationSiteFormProps {
  organizationId: string;
  onSuccess(): void;
}

export function CreateOrganizationSiteForm({
  organizationId,
  onSuccess,
}: CreateOrganizationSiteFormProps) {
  const mutation = useCreateOrganizationSite(organizationId);
  const { data: countries, isPending: countriesPending } = useCountries();

  const {
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<CreateOrganizationSiteFormValues>({
    resolver: zodResolver(createOrganizationSiteSchema),
    defaultValues: {
      name: "",
      type: "",
      countryId: "",
      statusDate: today(),
      addressLine1: "",
      city: "",
      postalCode: "",
    },
  });

  async function onSubmit(values: CreateOrganizationSiteFormValues) {
    await mutation.mutateAsync({
      name: values.name,
      type: values.type,
      countryId: values.countryId,
      statusDate: values.statusDate,
      addressLine1: values.addressLine1 || null,
      city: values.city || null,
      postalCode: values.postalCode || null,
    });

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
              <FieldLabel htmlFor="siteName">Site Name</FieldLabel>

              <Input id="siteName" placeholder="Hyderabad Plant" {...field} />

              <FieldError errors={[errors.name]} />
            </Field>
          )}
        />

        <Controller
          control={control}
          name="type"
          render={({ field }) => (
            <Field data-invalid={!!errors.type}>
              <FieldLabel htmlFor="siteType">Site Type</FieldLabel>

              <Select value={field.value} onValueChange={field.onChange}>
                <SelectTrigger id="siteType" className="w-full">
                  <SelectValue placeholder="Select a type" />
                </SelectTrigger>

                <SelectContent>
                  {ORGANIZATION_SITE_TYPES.map((type) => (
                    <SelectItem key={type.value} value={type.value}>
                      {type.label}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>

              <FieldError errors={[errors.type]} />
            </Field>
          )}
        />

        <Controller
          control={control}
          name="countryId"
          render={({ field }) => (
            <Field data-invalid={!!errors.countryId}>
              <FieldLabel htmlFor="siteCountry">Country</FieldLabel>

              <Select
                value={field.value}
                onValueChange={field.onChange}
                disabled={countriesPending}
              >
                <SelectTrigger id="siteCountry" className="w-full">
                  <SelectValue
                    placeholder={
                      countriesPending ? "Loading..." : "Select a country"
                    }
                  />
                </SelectTrigger>

                <SelectContent>
                  {(countries ?? []).map((country) => (
                    <SelectItem key={country.id} value={country.id}>
                      {country.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>

              <FieldError errors={[errors.countryId]} />
            </Field>
          )}
        />

        <Controller
          control={control}
          name="city"
          render={({ field }) => (
            <Field data-invalid={!!errors.city}>
              <FieldLabel htmlFor="siteCity">City</FieldLabel>

              <Input id="siteCity" placeholder="Hyderabad" {...field} />

              <FieldError errors={[errors.city]} />
            </Field>
          )}
        />

        <Controller
          control={control}
          name="statusDate"
          render={({ field }) => (
            <Field data-invalid={!!errors.statusDate}>
              <FieldLabel htmlFor="siteStatusDate">Opened</FieldLabel>

              <Input id="siteStatusDate" type="date" {...field} />

              <FieldError errors={[errors.statusDate]} />
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
          {mutation.isPending ? "Saving..." : "Add Site"}
        </Button>
      </div>
    </form>
  );
}
