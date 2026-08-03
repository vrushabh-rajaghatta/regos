import { useState } from "react";

import { Button } from "@/components/ui/button";

import { useStudyFilings } from "../hooks/useStudyFilings";

interface Props {
  studyId: string;
}

/**
 * *"Which filings cite this study?"* — the inverse of the application's own
 * list, and the half that makes a citation visible from both ends.
 *
 * Two kinds of answer, because there are two ways a filing names a study: the
 * **application** says the filing rests on it, and a **sequence** carries a
 * document that reports it. Neither implies the other — a study can be cited
 * before anything is filed about it, and a sequence can report a study the
 * application never cited.
 */
export function StudyFilings({ studyId }: Props) {
  const [asked, setAsked] = useState(false);

  const { data, isLoading, error } = useStudyFilings(asked ? studyId : null);

  if (!asked) {
    return (
      <Button
        size="sm"
        variant="ghost"
        className="mt-1 px-0"
        data-testid="show-study-filings"
        aria-label={`Show the filings that cite this study`}
        onClick={() => setAsked(true)}
      >
        Where is it filed?
      </Button>
    );
  }

  return (
    <div className="mt-2 text-sm" data-testid="study-filings">
      {isLoading && <p className="text-muted-foreground">Loading filings...</p>}

      {error && <p className="text-destructive">Failed to load filings.</p>}

      {data?.length === 0 && (
        <p className="text-muted-foreground" data-testid="study-filings-empty">
          Nothing cites this study yet.
        </p>
      )}

      <ul className="space-y-1">
        {data?.map((filing) => (
          <li
            key={`${filing.kind}-${filing.submissionId ?? filing.applicationId}`}
            data-testid="study-filing"
            className="flex flex-wrap items-baseline gap-2"
          >
            <span className="rounded bg-muted px-1.5 py-0.5 text-xs">
              {filing.kind === "Application" ? "Application" : "Sequence"}
            </span>

            <span>{filing.applicationName}</span>

            {filing.applicationNumber && (
              <span className="font-mono text-xs text-muted-foreground">
                {filing.applicationNumber}
              </span>
            )}

            {filing.kind === "Sequence" && (
              <span className="text-muted-foreground">
                {/* The screen's word for a Submission is a sequence, and a
                    draft has no number yet (ADR-044). */}
                {filing.sequenceNumber ?? "draft"}
                {filing.submissionTitle ? ` · ${filing.submissionTitle}` : ""}
              </span>
            )}
          </li>
        ))}
      </ul>
    </div>
  );
}
