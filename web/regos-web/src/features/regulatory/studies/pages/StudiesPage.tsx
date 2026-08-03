import { useState } from "react";

import { Button } from "@/components/ui/button";
import { Page } from "@/shared/components/Page";
import { PageHeader } from "@/shared/components/PageHeader";

import { RegisterStudyDialog } from "../components/RegisterStudyDialog";
import { StudyFilings } from "../components/StudyFilings";
import { studyKindLabel } from "../constants/studyKinds";
import { useStudies } from "../hooks/useStudies";

/**
 * The study registry.
 *
 * A study is a thing in the world that documents are *about*, and it exists
 * whether or not anything has been filed — which is why this list is a sibling
 * of Products rather than a page inside a submission.
 */
export function StudiesPage() {
  const [registering, setRegistering] = useState(false);

  const { data, isLoading, error } = useStudies();
  const rows = data ?? [];

  return (
    <Page>
      <PageHeader
        title="Studies"
        description="The studies your submissions report on, by the ID your organisation gives them."
        actions={
          <Button onClick={() => setRegistering(true)} data-testid="register-study">
            Register study
          </Button>
        }
      />

      {isLoading && <p className="text-muted-foreground">Loading studies...</p>}
      {error && <p className="text-destructive">Failed to load studies.</p>}

      {!isLoading && !error && rows.length === 0 && (
        <div
          className="rounded-lg border border-dashed p-8 text-center"
          data-testid="studies-empty"
        >
          <h3 className="text-lg font-semibold">No studies yet.</h3>
          <p className="mt-1 text-sm text-muted-foreground">
            A document filed in CTD 4.2.x or 5.3.x reports a study, and cannot be
            published until that study is registered here.
          </p>
        </div>
      )}

      <ul className="space-y-3">
        {rows.map((study) => (
          <li
            key={`${study.kind}-${study.id}`}
            className="rounded-lg border p-4"
            data-testid="study-row"
          >
            <div className="flex items-baseline gap-3">
              <span className="font-mono text-sm font-semibold">
                {study.sponsorStudyIdentifier}
              </span>

              <span className="text-xs text-muted-foreground">
                {studyKindLabel(study.kind)}
              </span>
            </div>

            <p className="mt-1 text-sm">{study.title}</p>

            {/* The inverse question, on the study's own row: "which filings
                cite this?" Fetched when asked for rather than with the list,
                because most of the time nobody is asking. */}
            <StudyFilings studyId={study.id} />
          </li>
        ))}
      </ul>

      <RegisterStudyDialog open={registering} onOpenChange={setRegistering} />
    </Page>
  );
}
