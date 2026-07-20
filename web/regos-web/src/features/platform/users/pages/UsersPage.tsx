import { useState } from "react";

import { PageHeader } from "@/shared/components/PageHeader";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";

import { InviteUserDialog } from "../components/InviteUserDialog";
import { UsersTable } from "../components/UsersTable";
import { useUsers } from "../hooks/useUsers";

const ALL_STATUSES = "All";
const PAGE_SIZE = 20;

export function UsersPage() {
  const [dialogOpen, setDialogOpen] = useState(false);
  const [search, setSearch] = useState("");
  const [status, setStatus] = useState(ALL_STATUSES);
  const [page, setPage] = useState(1);

  const { data, isPending, isError, refetch } = useUsers({
    search: search.trim() || undefined,
    status: status === ALL_STATUSES ? undefined : status,
    page,
    pageSize: PAGE_SIZE,
  });

  const isFiltered = search.trim() !== "" || status !== ALL_STATUSES;

  const totalPages = data
    ? Math.max(1, Math.ceil(data.totalCount / data.pageSize))
    : 1;

  return (
    <>
      <PageHeader
        title="Users"
        description="Invite and manage people in your organization."
        actions={
          <Button onClick={() => setDialogOpen(true)}>Invite User</Button>
        }
      />

      <InviteUserDialog open={dialogOpen} onOpenChange={setDialogOpen} />

      <div className="mt-6 flex flex-wrap items-center gap-3">
        <Input
          placeholder="Search by name or email"
          value={search}
          onChange={(event) => {
            setSearch(event.target.value);
            setPage(1);
          }}
          className="max-w-xs"
        />

        <Select
          value={status}
          onValueChange={(value) => {
            setStatus(value ?? ALL_STATUSES);
            setPage(1);
          }}
        >
          <SelectTrigger className="w-44">
            <SelectValue />
          </SelectTrigger>

          <SelectContent>
            <SelectItem value={ALL_STATUSES}>All statuses</SelectItem>
            <SelectItem value="Invited">Invited</SelectItem>
            <SelectItem value="Active">Active</SelectItem>
            <SelectItem value="Inactive">Inactive</SelectItem>
          </SelectContent>
        </Select>
      </div>

      <div className="mt-4">
        {/* Loading */}
        {isPending && (
          <div className="rounded-lg border p-8 text-center text-muted-foreground">
            Loading users...
          </div>
        )}

        {/* Error */}
        {!isPending && isError && (
          <div className="rounded-lg border border-destructive/40 p-8 text-center">
            <p className="text-sm text-destructive" role="alert">
              Could not load users. Check that the API is running.
            </p>

            <Button
              variant="outline"
              className="mt-3"
              onClick={() => refetch()}
            >
              Retry
            </Button>
          </div>
        )}

        {/* Empty */}
        {!isPending && !isError && data && data.items.length === 0 && (
          <div className="rounded-lg border border-dashed p-8 text-center text-muted-foreground">
            {isFiltered
              ? "No users match your search."
              : "No users found. Invite your first user to get started."}
          </div>
        )}

        {/* Success */}
        {!isPending && !isError && data && data.items.length > 0 && (
          <>
            <UsersTable users={data.items} />

            <div className="mt-3 flex items-center justify-between text-sm text-muted-foreground">
              <span>
                {data.totalCount} user{data.totalCount === 1 ? "" : "s"} &middot;
                page {data.page} of {totalPages}
              </span>

              <div className="flex gap-2">
                <Button
                  variant="outline"
                  disabled={data.page <= 1}
                  onClick={() => setPage((current) => Math.max(1, current - 1))}
                >
                  Previous
                </Button>

                <Button
                  variant="outline"
                  disabled={data.page >= totalPages}
                  onClick={() => setPage((current) => current + 1)}
                >
                  Next
                </Button>
              </div>
            </div>
          </>
        )}
      </div>
    </>
  );
}
