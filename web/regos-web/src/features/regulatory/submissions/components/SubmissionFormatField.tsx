import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";

import { useChangeSubmissionFormat } from "../hooks/useChangeSubmissionFormat";
import {
  FORMAT_DESCRIPTIONS,
  SUBMISSION_FORMATS,
  formatLabel,
  type SubmissionFormat,
} from "../utils/formatLabel";

interface Props {
  submissionId: string;
  format: string;
  /** Anything other than a draft renders read-only — see below. */
  status: string;
}

/**
 * What this filing will be rendered as.
 *
 * **Editable while a draft, and plain text afterwards** (ADR-047). The freeze
 * is enforced by the aggregate, not here; this only declines to offer the
 * control, so a user is never shown an action the server would refuse.
 */
export function SubmissionFormatField({ submissionId, format, status }: Props) {
  const mutation = useChangeSubmissionFormat(submissionId);

  const isDraft = status === "Draft";

  const items = Object.fromEntries(
    SUBMISSION_FORMATS.map((value) => [value, formatLabel(value)])
  );

  return (
    <div data-testid="submission-format" data-format={format}>
      <div className="text-sm text-muted-foreground">Submission Format</div>

      {isDraft ? (
        <>
          <div className="mt-1 max-w-xs">
            <Select
              items={items}
              value={format}
              // Clearing the select is not a format, so it is not a change.
              onValueChange={(next) => next && mutation.mutate(next)}
            >
              <SelectTrigger
                id="submission-format"
                className="w-full"
                disabled={mutation.isPending}
              >
                <SelectValue />
              </SelectTrigger>

              <SelectContent>
                {SUBMISSION_FORMATS.map((value) => (
                  <SelectItem key={value} value={value}>
                    {formatLabel(value)}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          <p className="mt-1 text-sm text-muted-foreground">
            {FORMAT_DESCRIPTIONS[format as SubmissionFormat]}
          </p>
        </>
      ) : (
        <>
          <div className="font-medium">{formatLabel(format)}</div>

          {/* Not a disabled control: the sequence has been filed, and what it
              was filed as is no longer a choice anyone can make. */}
          <p className="mt-1 text-sm text-muted-foreground">
            Fixed when the sequence was published.
          </p>
        </>
      )}

      {mutation.isError && (
        <p className="mt-1 text-sm text-destructive">
          {mutation.error.message}
        </p>
      )}
    </div>
  );
}
