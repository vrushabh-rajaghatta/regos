import { useState } from "react";

import { PageHeader } from "@/shared/components/PageHeader";
import { Button } from "@/components/ui/button";

import { InviteUserDialog } from "../components/InviteUserDialog";

export function UsersPage() {
  const [dialogOpen, setDialogOpen] = useState(false);

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

      {/* A user directory query is a separate capability; invited users are
          persisted in the Invited state and will be listed here once it lands. */}
      <div className="mt-6 rounded-lg border border-dashed p-8 text-center text-muted-foreground">
        Invite a user to add them to an organization. They start in the{" "}
        <span className="font-medium">Invited</span> state.
      </div>
    </>
  );
}
