import { useState } from "react";
import { useParams } from "react-router-dom";

import { Page } from "@/shared/components/Page";
import { PageHeader } from "@/shared/components/PageHeader";

import { ContentPlanSectionTree } from "../components/ContentPlanSectionTree";
import { PlaceDocumentDialog } from "../components/PlaceDocumentDialog";
import { ReportStudyDialog } from "../components/ReportStudyDialog";
import { useSubmission } from "../hooks/useSubmission";
import { useSubmissionContentPlan } from "../hooks/useSubmissionContentPlan";
import { usePlaceSubmissionDocument } from "../hooks/usePlaceSubmissionDocument";

/**
 * The dossier builder.
 *
 * Composes one read model — the content plan — and interprets none of it. What
 * is satisfied, what is supporting content, and how full the dossier is are
 * answered by the server; this page renders those answers and offers the one
 * action that changes them.
 *
 * Validation findings deliberately live on their own tab. Documents is the
 * dossier's inventory; this is its structure.
 */
export function SubmissionContentPlanPage() {
  const { submissionId } = useParams();

  const [target, setTarget] = useState<{ id: string; label: string } | null>(
    null
  );

  const [reporting, setReporting] = useState<{
    submissionDocumentId: string;
    documentName: string;
  } | null>(null);

  const submission = useSubmission(submissionId!);
  const { data, isLoading, error } = useSubmissionContentPlan(submissionId!);
  const place = usePlaceSubmissionDocument(submissionId!);

  // Only a draft dossier may change — mirrors the aggregate invariant.
  const editable = submission.data?.status === "Draft";

  const progress = data?.progress;

  return (
    <Page>
      <PageHeader
        title="Content Plan"
        description="Where each document sits in the dossier, and what is still expected."
      />

      {isLoading && (
        <p className="text-muted-foreground">Loading the content plan...</p>
      )}

      {!isLoading && error && (
        <p className="text-destructive">Failed to load the content plan.</p>
      )}

      {!isLoading && !error && data && (
        <div className="space-y-6">
          {/*
            No blueprint governs this submission — a legitimate state, not a
            failure, so it is rendered rather than reported as an error.
          */}
          {data.boundTemplateVersionId === null ? (
            <div
              className="rounded-lg border border-dashed p-8 text-center"
              data-testid="content-plan-unbound"
            >
              <h3 className="text-lg font-semibold">
                No dossier template governs this submission.
              </h3>
              <p className="mt-1 text-sm text-muted-foreground">
                Its documents can still be attached and it can still be
                published; there is simply no structure to place them into.
              </p>
            </div>
          ) : (
            <p
              className="text-sm text-muted-foreground"
              data-testid="content-plan-progress"
            >
              <span className="font-medium text-foreground">
                {progress!.satisfied} of {progress!.placeholders} placeholders
                filled
              </span>{" "}
              · {data.templateName} v{data.versionNumber}
            </p>
          )}

          {data.unplacedDocuments.length > 0 && (
            <section
              className="rounded-lg border border-amber-600/40 bg-amber-50 p-4 dark:bg-amber-950/20"
              data-testid="unplaced-documents"
            >
              <h2 className="font-medium">
                Not yet placed ({data.unplacedDocuments.length})
              </h2>
              <p className="mt-1 text-sm text-muted-foreground">
                Attached to this submission but sitting nowhere in the dossier,
                so they satisfy nothing.
              </p>

              <ul className="mt-3 space-y-1 text-sm">
                {data.unplacedDocuments.map((document) => (
                  <li key={document.submissionDocumentId}>
                    <span className="font-medium">{document.name}</span>{" "}
                    <span className="text-muted-foreground">
                      {document.documentTypeName}
                    </span>
                  </li>
                ))}
              </ul>
            </section>
          )}

          {data.sections.length > 0 && (
            <ContentPlanSectionTree
              sections={data.sections}
              editable={editable}
              onPlace={setTarget}
              onRemove={(submissionDocumentId) =>
                place.mutate({ submissionDocumentId, templateSectionId: null })
              }
              onReportStudy={setReporting}
            />
          )}

          {place.isError && (
            <p className="text-sm text-destructive">
              {(place.error as Error).message}
            </p>
          )}

          {!editable && submission.data && (
            <p className="text-sm text-muted-foreground">
              This submission is no longer a draft. Its dossier is read-only.
            </p>
          )}
        </div>
      )}

      {editable && (
        <PlaceDocumentDialog
          submissionId={submissionId!}
          section={target}
          unplaced={data?.unplacedDocuments ?? []}
          onOpenChange={(open) => !open && setTarget(null)}
        />
      )}

      {editable && (
        <ReportStudyDialog
          submissionId={submissionId!}
          placement={reporting}
          onOpenChange={(open) => !open && setReporting(null)}
        />
      )}
    </Page>
  );
}
