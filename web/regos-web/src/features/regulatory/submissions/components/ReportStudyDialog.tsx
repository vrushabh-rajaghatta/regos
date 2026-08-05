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
import { useFileTags } from "../hooks/useFileTags";
import { useReportStudyOnPlacement } from "../hooks/useReportStudyOnPlacement";

interface Props {
  submissionId: string;
  /** The placement being described, or null when the dialog is closed. */
  placement: {
    submissionDocumentId: string;
    documentName: string;
    studyId: string | null;
    fileTag: string | null;
  } | null;
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
  // Opened on a placement that already says something: start from what it says
  // rather than from blank, or "Save" silently erases the other half.
  //
  // Initialised rather than synchronised in an effect. The parent keys this
  // component on the placement, so a different placement is a different
  // component and these run again — React's own answer to resetting state,
  // and one that cannot render once with the previous placement's values.
  const [selected, setSelected] = useState(placement?.studyId ?? "");
  const [fileTag, setFileTag] = useState(placement?.fileTag ?? "");

  const studies = useStudies();
  const fileTags = useFileTags();
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
        fileTag: fileTag === "" ? null : fileTag,
      });
    } catch {
      // A refusal is an outcome. The server's reason renders below.
      return;
    }

    setSelected("");
    setFileTag("");
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

          {/*
            Offered only alongside a study, because a file tag says what a
            document contributes to a study report — with no study it describes
            nothing, and the server refuses it.
          */}
          {selected !== "" && (
            <Field>
              <FieldLabel htmlFor="fileTag">Role in the study report</FieldLabel>

              <select
                id="fileTag"
                value={fileTag}
                onChange={(event) => setFileTag(event.target.value)}
                className="h-9 w-full rounded-md border bg-transparent px-3 text-sm"
                data-testid="file-tag-select"
              >
                <option value="">Not stated</option>
                {(fileTags.data ?? []).map((tag) => (
                  <option key={tag.name} value={tag.name}>
                    {tag.name}
                    {tag.realm === "ich" ? "" : ` (${tag.realm})`}
                  </option>
                ))}
              </select>

              <p className="text-xs text-muted-foreground">
                {/*
                  Shown as published, not prettified: this is the token the STF
                  writes and the reviewer's tool matches on. Which one belongs
                  on a given document is the filer's judgement — RegOS has the
                  words, not the rule for choosing between them.
                */}
                ICH publishes 97. Tags marked <code>us</code> or <code>jp</code>{" "}
                are regional.
              </p>
            </Field>
          )}

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
