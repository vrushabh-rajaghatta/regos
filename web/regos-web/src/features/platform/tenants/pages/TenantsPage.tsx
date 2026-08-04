import { useState } from "react";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { PageHeader } from "@/shared/components/PageHeader";

import {
  useActivateTenant,
  useCreateTenant,
  useDeactivateTenant,
  useTenants,
} from "../hooks/useTenants";
import type { CreateTenantRequest } from "../types/CreateTenantRequest";

export function TenantsPage() {
  const [dialogOpen, setDialogOpen] = useState(false);

  const { data, isPending, isError, refetch } = useTenants();
  const activate = useActivateTenant();
  const deactivate = useDeactivateTenant();

  return (
    <>
      <PageHeader
        title="Tenants"
        description="Provision and maintain the customers of the platform."
        actions={
          <Button onClick={() => setDialogOpen(true)}>Create Tenant</Button>
        }
      />

      <CreateTenantDialog open={dialogOpen} onOpenChange={setDialogOpen} />

      <div className="mt-6">
        {isPending && (
          <div className="rounded-lg border p-8 text-center text-muted-foreground">
            Loading tenants...
          </div>
        )}

        {!isPending && isError && (
          <div className="rounded-lg border border-destructive/40 p-8 text-center">
            <p className="text-sm text-destructive" role="alert">
              Could not load tenants. Check that the API is running.
            </p>
            <Button variant="outline" className="mt-3" onClick={() => refetch()}>
              Retry
            </Button>
          </div>
        )}

        {!isPending && !isError && data && (
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b text-left text-muted-foreground">
                <th className="py-2 pr-4 font-medium">Name</th>
                <th className="py-2 pr-4 font-medium">Status</th>
                <th className="py-2 font-medium" />
              </tr>
            </thead>
            <tbody>
              {data.map((tenant) => (
                <tr key={tenant.id} className="border-b" data-testid="tenant-row">
                  <td className="py-2 pr-4">{tenant.name}</td>
                  <td className="py-2 pr-4">
                    <Badge
                      variant={
                        tenant.status === "Active" ? "default" : "secondary"
                      }
                    >
                      {tenant.status}
                    </Badge>
                  </td>
                  <td className="py-2 text-right">
                    {tenant.status === "Active" ? (
                      <Button
                        variant="outline"
                        size="sm"
                        disabled={deactivate.isPending}
                        onClick={() => deactivate.mutate(tenant.id)}
                      >
                        Deactivate
                      </Button>
                    ) : (
                      <Button
                        variant="outline"
                        size="sm"
                        disabled={activate.isPending}
                        onClick={() => activate.mutate(tenant.id)}
                      >
                        Activate
                      </Button>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </>
  );
}

function CreateTenantDialog({
  open,
  onOpenChange,
}: {
  open: boolean;
  onOpenChange: (open: boolean) => void;
}) {
  const create = useCreateTenant();

  const [form, setForm] = useState<CreateTenantRequest>({
    name: "",
    adminEmail: "",
    adminFirstName: "",
    adminLastName: "",
  });

  const set = (patch: Partial<CreateTenantRequest>) =>
    setForm((current) => ({ ...current, ...patch }));

  const submit = (event: React.FormEvent) => {
    event.preventDefault();

    create.mutate(form, {
      onSuccess: () => {
        onOpenChange(false);
        setForm({
          name: "",
          adminEmail: "",
          adminFirstName: "",
          adminLastName: "",
        });
      },
    });
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Create Tenant</DialogTitle>
          <DialogDescription>
            Provisions the tenant and invites its first administrator — they
            set their password by accepting the invitation; no password is
            created here. Their organization registry starts empty: they
            record their own company, and everyone they work with.
          </DialogDescription>
        </DialogHeader>

        <form onSubmit={submit} className="space-y-4">
          <div className="space-y-1.5">
            <Label htmlFor="tenant-name">Tenant name</Label>
            <Input
              id="tenant-name"
              value={form.name}
              onChange={(e) => set({ name: e.target.value })}
              required
            />
          </div>

          <div className="grid grid-cols-2 gap-3">
            <div className="space-y-1.5">
              <Label htmlFor="admin-first">Admin first name</Label>
              <Input
                id="admin-first"
                value={form.adminFirstName}
                onChange={(e) => set({ adminFirstName: e.target.value })}
                required
              />
            </div>
            <div className="space-y-1.5">
              <Label htmlFor="admin-last">Admin last name</Label>
              <Input
                id="admin-last"
                value={form.adminLastName}
                onChange={(e) => set({ adminLastName: e.target.value })}
                required
              />
            </div>
          </div>

          <div className="space-y-1.5">
            <Label htmlFor="admin-email">Admin email</Label>
            <Input
              id="admin-email"
              type="email"
              value={form.adminEmail}
              onChange={(e) => set({ adminEmail: e.target.value })}
              required
            />
          </div>

          {create.isError && (
            <p className="text-sm text-destructive" role="alert">
              {create.error.message}
            </p>
          )}

          <div className="flex justify-end gap-2">
            <Button
              type="button"
              variant="outline"
              onClick={() => onOpenChange(false)}
            >
              Cancel
            </Button>
            <Button type="submit" disabled={create.isPending}>
              {create.isPending ? "Creating..." : "Create Tenant"}
            </Button>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  );
}
