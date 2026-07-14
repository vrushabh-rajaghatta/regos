import { useState } from "react";
import { useParams } from "react-router-dom";

import { Button } from "@/components/ui/button";
import { useSubmissions } from "@/features/regulatory/submissions/hooks/useSubmissions";
import { SubmissionCard } from "@/features/regulatory/submissions/components/SubmissionCard";
import { CreateSubmissionDialog } from "@/features/regulatory/submissions/components/CreateSubmissionDialog";

import { useApplication } from "../hooks/useApplication";

export function ApplicationSubmissionsPage() {
  const { productId, applicationId } = useParams();

  const [dialogOpen, setDialogOpen] = useState(false);

  // The create form scopes submission types to the application's authority,
  // so we need the application loaded before creating.
  const { data: application } = useApplication(applicationId!);

  const { data, isLoading, error } = useSubmissions(applicationId!);

  return (
    <div className="space-y-4 p-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold">Submissions</h1>

        <Button onClick={() => setDialogOpen(true)} disabled={!application}>
          New Submission
        </Button>
      </div>

      {application && (
        <CreateSubmissionDialog
          productId={productId!}
          applicationId={applicationId!}
          authorityId={application.authorityId}
          open={dialogOpen}
          onOpenChange={setDialogOpen}
        />
      )}

      {isLoading && (
        <p className="text-muted-foreground">Loading Submissions...</p>
      )}

      {!isLoading && error && (
        <p className="text-destructive">Failed to load Submissions.</p>
      )}

      {!isLoading && !error && data?.length === 0 && (
        <div className="rounded-lg border border-dashed p-12 text-center">
          <h3 className="text-lg font-semibold">No Submissions</h3>

          <p className="mt-2 text-sm text-muted-foreground">
            Create your first Submission for this Application.
          </p>

          <Button
            className="mt-4"
            onClick={() => setDialogOpen(true)}
            disabled={!application}
          >
            New Submission
          </Button>
        </div>
      )}

      {!isLoading && !error && data && data.length > 0 && (
        <div className="space-y-3">
          {data.map((submission) => (
            <SubmissionCard
              key={submission.id}
              productId={productId!}
              applicationId={applicationId!}
              submission={submission}
            />
          ))}
        </div>
      )}
    </div>
  );
}
