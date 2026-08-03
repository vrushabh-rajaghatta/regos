import { useState } from "react";

import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Field, FieldLabel } from "@/components/ui/field";

import { studyKindLabel, useStudies } from "../../studies";
import { useReportStudyOnPlacement } from "../hooks/useReportStudyOnPlacement";

interface Props {
  submissionId: string;
  /** The placement being described, or null when the dialog is closed. */
  placement: { submissionDocumentId: string; documentName: string } | null;
  onOpenChange(open: boolean): void;
}

/**
 * Names the study a placement reports.
 *
 * A picker, not a form: it selects an existing study rather than collecting
 * typed input, which is why there is no zod schema (SC-103's own exception).
 * Registering a study is a separate gesture on its own screen — the registry is
 * tenant-wide and a study outlives any one filing.
 */
export function ReportStudyDialog({
  submissionId,
  placement,
  onOpenChange,
}: Props) {
  const [selected, setSelected] = useState("");

  const studies = useStudies();
  const report = useReportStudyOnPlacement(submissionId);

  async function submit(event: React.FormEvent) {
    event.preventDefault();

    if (!placement) return;

    // The option's value carries both facts, because the id alone does not say
    // which of the two aggregates it belongs to.
    const chosen = (studies.data ?? []).find((s) => s.id === selected);

    try {
      await report.mutateAsync({
        submissionDocumentId: placement.submissionDocumentId,
        study: chosen ? { id: chosen.id, kind: chosen.kind } : null,
      });
    } catch {
      // A refusal is an outcome. The server's reason renders below.
      return;
    }

    setSelected("");
    onOpenChange(false);
  }

  return (
    <Dialog open={placement !== null} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Which study does this report?</DialogTitle>
        </DialogHeader>

        <form onSubmit={submit} className="space-y-4">
          <p className="text-sm text-muted-foreground">
            {placement?.documentName}
          </p>

          <Field>
            <FieldLabel htmlFor="reportedStudy">Study</FieldLabel>

            <select
              id="reportedStudy"
              value={selected}
              onChange={(event) => setSelected(event.target.value)}
              className="h-9 w-full rounded-md border bg-transparent px-3 text-sm"
            >
              <option value="">Reports no study</option>
              {(studies.data ?? []).map((study) => (
                <option key={study.id} value={study.id}>
                  {study.sponsorStudyIdentifier} — {study.title} (
                  {studyKindLabel(study.kind)})
                </option>
              ))}
            </select>

            {studies.data?.length === 0 && (
              <p className="text-xs text-muted-foreground">
                No studies are registered yet. A document filed in CTD 4.2.x or
                5.3.x reports one, and it is registered under Studies.
              </p>
            )}
          </Field>

          {report.isError && (
            <p
              className="text-sm text-destructive"
              data-testid="report-study-error"
            >
              {(report.error as Error).message}
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

            <Button type="submit" disabled={report.isPending}>
              {report.isPending ? "Saving..." : "Save"}
            </Button>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  );
}
