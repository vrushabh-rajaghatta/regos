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

import { useContactRoles } from "../hooks/useContactRoles";
import { useCreateContact } from "../hooks/useCreateContact";
import { useOrganizationSites } from "../hooks/useOrganizationSites";
import { today } from "../utils/today";
import {
  createContactSchema,
  type CreateContactFormValues,
} from "../validation/createContactSchema";

interface CreateContactFormProps {
  organizationId: string;
  onSuccess(): void;
}

/**
 * One role and one email, not the collections the aggregate supports. The
 * server accepts many of each; offering a repeater before anyone has asked for
 * a second would be building the general case on speculation (ADR-018).
 */
export function CreateContactForm({
  organizationId,
  onSuccess,
}: CreateContactFormProps) {
  const mutation = useCreateContact(organizationId);
  const { data: roles } = useContactRoles();
  const { data: sites } = useOrganizationSites(organizationId);

  const {
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<CreateContactFormValues>({
    resolver: zodResolver(createContactSchema),
    defaultValues: {
      firstName: "",
      lastName: "",
      statusDate: today(),
      title: "",
      department: "",
      organizationSiteId: "",
      roleId: "",
      email: "",
      phone: "",
    },
  });

  async function onSubmit(values: CreateContactFormValues) {
    try {
      await mutation.mutateAsync({
        firstName: values.firstName,
        lastName: values.lastName,
        statusDate: values.statusDate,
        title: values.title || null,
        department: values.department || null,
        organizationSiteId: values.organizationSiteId || null,
        roleIds: values.roleId ? [values.roleId] : [],
        emails: values.email ? [values.email] : [],
        phones: values.phone ? [values.phone] : [],
      });
    } catch {
      // A refusal is an outcome, not a crash — the server's reason is rendered
      // from mutation.error below.
      return;
    }

    onSuccess();
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
      <FieldGroup>
        <Controller
          control={control}
          name="firstName"
          render={({ field }) => (
            <Field data-invalid={!!errors.firstName}>
              <FieldLabel htmlFor="contactFirstName">First Name</FieldLabel>

              <Input id="contactFirstName" {...field} />

              <FieldError errors={[errors.firstName]} />
            </Field>
          )}
        />

        <Controller
          control={control}
          name="lastName"
          render={({ field }) => (
            <Field data-invalid={!!errors.lastName}>
              <FieldLabel htmlFor="contactLastName">Last Name</FieldLabel>

              <Input id="contactLastName" {...field} />

              <FieldError errors={[errors.lastName]} />
            </Field>
          )}
        />

        <Controller
          control={control}
          name="title"
          render={({ field }) => (
            <Field data-invalid={!!errors.title}>
              <FieldLabel htmlFor="contactTitle">Title</FieldLabel>

              <Input
                id="contactTitle"
                placeholder="Qualified Person"
                {...field}
              />

              <FieldError errors={[errors.title]} />
            </Field>
          )}
        />

        <Controller
          control={control}
          name="roleId"
          render={({ field }) => (
            <Field data-invalid={!!errors.roleId}>
              <FieldLabel htmlFor="contactRole">Role</FieldLabel>

              <Select value={field.value} onValueChange={field.onChange}>
                <SelectTrigger id="contactRole" className="w-full">
                  <SelectValue placeholder="Select a role" />
                </SelectTrigger>

                <SelectContent>
                  {(roles ?? []).map((role) => (
                    <SelectItem key={role.id} value={role.id}>
                      {role.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>

              <FieldError errors={[errors.roleId]} />
            </Field>
          )}
        />

        <Controller
          control={control}
          name="organizationSiteId"
          render={({ field }) => (
            <Field data-invalid={!!errors.organizationSiteId}>
              <FieldLabel htmlFor="contactSite">Site</FieldLabel>

              <Select value={field.value} onValueChange={field.onChange}>
                <SelectTrigger id="contactSite" className="w-full">
                  <SelectValue placeholder="No specific site" />
                </SelectTrigger>

                <SelectContent>
                  {(sites ?? []).map((site) => (
                    <SelectItem key={site.siteId} value={site.siteId}>
                      {site.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>

              <FieldError errors={[errors.organizationSiteId]} />
            </Field>
          )}
        />

        <Controller
          control={control}
          name="email"
          render={({ field }) => (
            <Field data-invalid={!!errors.email}>
              <FieldLabel htmlFor="contactEmail">Email</FieldLabel>

              <Input id="contactEmail" {...field} />

              <FieldError errors={[errors.email]} />
            </Field>
          )}
        />

        <Controller
          control={control}
          name="statusDate"
          render={({ field }) => (
            <Field data-invalid={!!errors.statusDate}>
              <FieldLabel htmlFor="contactStatusDate">Appointed</FieldLabel>

              <Input id="contactStatusDate" type="date" {...field} />

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
          {mutation.isPending ? "Saving..." : "Add Contact"}
        </Button>
      </div>
    </form>
  );
}
