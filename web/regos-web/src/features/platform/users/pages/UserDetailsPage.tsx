import { useState } from "react";
import { Link, useParams } from "react-router-dom";

import { Button, buttonVariants } from "@/components/ui/button";
import { PageHeader } from "@/shared/components/PageHeader";

import { UserNotFoundError } from "../api/getUser";
import { DeactivateUserDialog } from "../components/DeactivateUserDialog";
import { EditUserProfileDialog } from "../components/EditUserProfileDialog";
import { UserStatusBadge } from "../components/UserStatusBadge";
import { useActivateUser } from "../hooks/useActivateUser";
import { useResendInvitation } from "../hooks/useResendInvitation";
import { useUser } from "../hooks/useUser";

function DetailRow({ label, value }: { label: string; value: React.ReactNode }) {
  return (
    <div className="flex flex-col gap-1 border-b py-3 last:border-0 sm:flex-row sm:gap-4">
      <dt className="w-40 shrink-0 text-sm text-muted-foreground">{label}</dt>
      <dd className="text-sm">{value}</dd>
    </div>
  );
}

export function UserDetailsPage() {
  const { userId } = useParams<{ userId: string }>();
  const [editOpen, setEditOpen] = useState(false);
  const [deactivateOpen, setDeactivateOpen] = useState(false);
  const activate = useActivateUser(userId ?? "");
  const resend = useResendInvitation(userId ?? "");

  const { data, isPending, isError, error, refetch } = useUser(userId ?? "");

  const notFound = error instanceof UserNotFoundError;

  return (
    <>
      <PageHeader
        title={data ? `${data.firstName} ${data.lastName}` : "User"}
        description="User details."
        actions={
          <div className="flex gap-2">
            <Link
              to="/platform/users"
              className={buttonVariants({ variant: "outline" })}
            >
              Back to Users
            </Link>

            {data && (
              <Button variant="outline" onClick={() => setEditOpen(true)}>
                Edit
              </Button>
            )}

            {/* Deactivation applies to Active and Invited users (revoking an
                unaccepted invitation); hidden once already inactive. */}
            {data && data.status !== "Inactive" && (
              <Button
                variant="outline"
                onClick={() => setDeactivateOpen(true)}
              >
                Deactivate User
              </Button>
            )}

            {/* Resending applies only to someone still waiting to accept. It
                issues a new link and invalidates the previous one. */}
            {data && data.status === "Invited" && (
              <Button
                variant="outline"
                onClick={() => resend.mutate()}
                disabled={resend.isPending}
              >
                {resend.isPending ? "Sending..." : "Resend Invitation"}
              </Button>
            )}

            {/* Activation now means reinstatement, and nothing else. An invited
                user becomes active by accepting their invitation - activating
                them here was the only way to reach Active with no password, and
                ADR-027 closed it. */}
            {data && data.status === "Inactive" && (
              <Button
                onClick={() => activate.mutate()}
                disabled={activate.isPending}
              >
                {activate.isPending ? "Activating..." : "Reactivate User"}
              </Button>
            )}
          </div>
        }
      />

      {data && (
        <EditUserProfileDialog
          user={data}
          open={editOpen}
          onOpenChange={setEditOpen}
        />
      )}

      {data && (
        <DeactivateUserDialog
          userId={data.id}
          open={deactivateOpen}
          onOpenChange={setDeactivateOpen}
        />
      )}

      <div className="mt-6">
        {/* Loading */}
        {isPending && (
          <div className="rounded-lg border p-8 text-center text-muted-foreground">
            Loading user...
          </div>
        )}

        {/* Not found */}
        {!isPending && notFound && (
          <div className="rounded-lg border border-dashed p-8 text-center text-muted-foreground">
            <p>This user no longer exists, or is outside your organization.</p>

            <Link
              to="/platform/users"
              className={buttonVariants({ variant: "outline", className: "mt-3" })}
            >
              Back to Users
            </Link>
          </div>
        )}

        {/* Error */}
        {!isPending && isError && !notFound && (
          <div className="rounded-lg border border-destructive/40 p-8 text-center">
            <p className="text-sm text-destructive" role="alert">
              Could not load this user. Check that the API is running.
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

        {/* Success */}
        {!isPending && !isError && data && (
          <div className="rounded-lg border p-6">
            <dl>
              <DetailRow
                label="Full name"
                value={`${data.firstName} ${data.lastName}`}
              />
              <DetailRow label="Email" value={data.email} />
              <DetailRow
                label="Status"
                value={<UserStatusBadge status={data.status} />}
              />
              <DetailRow
                label="Created"
                value={new Date(data.createdOn).toLocaleString()}
              />
            </dl>

            {activate.isError && (
              <p className="mt-4 text-sm text-destructive" role="alert">
                {activate.error.message}
              </p>
            )}

          </div>
        )}
      </div>
    </>
  );
}
