import { useMemo } from "react";
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

import {
  useAuthorities,
  useCountries,
  useOrganizations,
} from "@/features/regulatory/masterData";

import { useApplicationTypes } from "../hooks/useApplicationTypes";
import { useCreateRegulatoryApplication } from "../hooks/useCreateRegulatoryApplication";
import {
  registerRegulatoryApplicationSchema,
  type RegisterRegulatoryApplicationFormValues,
} from "../validation/registerRegulatoryApplicationSchema";

interface Props {
  globalProductId: string;
  onSuccess: () => void;
}

export function RegisterRegulatoryApplicationForm({
  globalProductId,
  onSuccess,
}: Props) {
  const countriesQuery = useCountries();
  const authoritiesQuery = useAuthorities();
  const organizationsQuery = useOrganizations();

  const mutation = useCreateRegulatoryApplication(globalProductId);

  const {
    control,
    handleSubmit,
    watch,
    setValue,
    reset,
    formState: { errors },
  } = useForm<RegisterRegulatoryApplicationFormValues>({
    resolver: zodResolver(registerRegulatoryApplicationSchema),
    defaultValues: {
      name: "",
      countryId: "",
      authorityId: "",
      applicationTypeId: "",
      applicantOrganizationId: "",
    },
  });

  const selectedCountryId = watch("countryId");
  const selectedAuthorityId = watch("authorityId");

  // Application types are scoped to the authority, and the domain enforces
  // that: RegulatoryApplication.Create refuses a type belonging to a different
  // authority. Fetching per-authority means the form cannot offer a choice the
  // domain would reject.
  const applicationTypesQuery = useApplicationTypes(selectedAuthorityId);

  // Authority list is filtered client-side by the selected country — no
  // backend call, since the full list is already loaded.
  const filteredAuthorities = useMemo(
    () =>
      (authoritiesQuery.data ?? []).filter(
        (authority) => authority.countryId === selectedCountryId,
      ),
    [authoritiesQuery.data, selectedCountryId],
  );

  // Mirror the backend rule: only active organizations may be selected.
  const activeOrganizations = useMemo(
    () =>
      (organizationsQuery.data ?? []).filter(
        (organization) => organization.status === "Active",
      ),
    [organizationsQuery.data],
  );

  // Value->label maps so the Select triggers display the chosen label
  // (e.g. "United States (US)") rather than the raw id.
  const countryItems = useMemo(
    () =>
      Object.fromEntries(
        (countriesQuery.data ?? []).map((country) => [
          country.id,
          `${country.name} (${country.code})`,
        ]),
      ),
    [countriesQuery.data],
  );

  const authorityItems = useMemo(
    () =>
      Object.fromEntries(
        (authoritiesQuery.data ?? []).map((authority) => [
          authority.id,
          `${authority.name} (${authority.code})`,
        ]),
      ),
    [authoritiesQuery.data],
  );

  const applicationTypeItems = useMemo(
    () =>
      Object.fromEntries(
        (applicationTypesQuery.data ?? []).map((type) => [type.id, type.name]),
      ),
    [applicationTypesQuery.data],
  );

  const organizationItems = useMemo(
    () =>
      Object.fromEntries(
        (organizationsQuery.data ?? []).map((organization) => [
          organization.id,
          organization.legalName,
        ]),
      ),
    [organizationsQuery.data],
  );

  async function onSubmit(values: RegisterRegulatoryApplicationFormValues) {
    await mutation.mutateAsync({
      name: values.name,
      countryId: values.countryId,
      authorityId: values.authorityId,
      applicationTypeId: values.applicationTypeId,
      applicantOrganizationId: values.applicantOrganizationId,
    });

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
              <FieldLabel htmlFor="name">Application Name</FieldLabel>

              <Input id="name" placeholder="e.g. US 510(k)" {...field} />

              <FieldError errors={[errors.name]} />
            </Field>
          )}
        />

        <Controller
          control={control}
          name="countryId"
          render={({ field }) => (
            <Field data-invalid={!!errors.countryId}>
              <FieldLabel htmlFor="countryId">Country</FieldLabel>

              <Select
                items={countryItems}
                value={field.value}
                onValueChange={(value) => {
                  field.onChange(value);
                  // Changing country invalidates the authority, and the
                  // application type hanging off it.
                  setValue("authorityId", "", { shouldValidate: false });
                  setValue("applicationTypeId", "", { shouldValidate: false });
                }}
              >
                <SelectTrigger id="countryId" className="w-full">
                  <SelectValue placeholder="Select country" />
                </SelectTrigger>

                <SelectContent>
                  {(countriesQuery.data ?? []).map((country) => (
                    <SelectItem key={country.id} value={country.id}>
                      {country.name} ({country.code})
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
          name="authorityId"
          render={({ field }) => (
            <Field data-invalid={!!errors.authorityId}>
              <FieldLabel htmlFor="authorityId">Authority</FieldLabel>

              <Select
                items={authorityItems}
                value={field.value}
                onValueChange={(value) => {
                  field.onChange(value);
                  setValue("applicationTypeId", "", { shouldValidate: false });
                }}
                disabled={!selectedCountryId}
              >
                <SelectTrigger id="authorityId" className="w-full">
                  <SelectValue
                    placeholder={
                      selectedCountryId
                        ? "Select authority"
                        : "Select a country first"
                    }
                  />
                </SelectTrigger>

                <SelectContent>
                  {filteredAuthorities.map((authority) => (
                    <SelectItem key={authority.id} value={authority.id}>
                      {authority.name} ({authority.code})
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>

              <FieldError errors={[errors.authorityId]} />
            </Field>
          )}
        />

        <Controller
          control={control}
          name="applicationTypeId"
          render={({ field }) => (
            <Field data-invalid={!!errors.applicationTypeId}>
              <FieldLabel htmlFor="applicationTypeId">
                Application Type
              </FieldLabel>

              <Select
                items={applicationTypeItems}
                value={field.value}
                onValueChange={field.onChange}
                disabled={!selectedAuthorityId}
              >
                <SelectTrigger id="applicationTypeId" className="w-full">
                  <SelectValue
                    placeholder={
                      selectedAuthorityId
                        ? "Select application type"
                        : "Select an authority first"
                    }
                  />
                </SelectTrigger>

                <SelectContent>
                  {(applicationTypesQuery.data ?? []).map((type) => (
                    <SelectItem key={type.id} value={type.id}>
                      {type.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>

              <p className="text-sm text-muted-foreground">
                What kind of application this is. Every sequence filed under it
                inherits this classification.
              </p>

              <FieldError errors={[errors.applicationTypeId]} />
            </Field>
          )}
        />

        <Controller
          control={control}
          name="applicantOrganizationId"
          render={({ field }) => (
            <Field data-invalid={!!errors.applicantOrganizationId}>
              <FieldLabel htmlFor="applicantOrganizationId">
                Applicant Organization
              </FieldLabel>

              <Select
                items={organizationItems}
                value={field.value}
                onValueChange={field.onChange}
              >
                <SelectTrigger id="applicantOrganizationId" className="w-full">
                  <SelectValue placeholder="Select organization" />
                </SelectTrigger>

                <SelectContent>
                  {activeOrganizations.map((organization) => (
                    <SelectItem key={organization.id} value={organization.id}>
                      {organization.legalName}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>

              <FieldError errors={[errors.applicantOrganizationId]} />
            </Field>
          )}
        />
      </FieldGroup>

      {mutation.isError && (
        <p className="text-sm font-normal text-destructive">
          Failed to create Application.
        </p>
      )}

      <div className="flex justify-end gap-2">
        <Button type="submit" disabled={mutation.isPending}>
          {mutation.isPending ? "Creating..." : "Create Application"}
        </Button>
      </div>
    </form>
  );
}
