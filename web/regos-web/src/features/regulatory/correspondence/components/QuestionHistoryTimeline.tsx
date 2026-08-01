import type { QuestionHistoryEntry } from "../types/CorrespondenceDetail";

interface QuestionHistoryTimelineProps {
  history: QuestionHistoryEntry[];
}

/**
 * A question's dated history — the third of these in RegOS, after
 * `RegistrationHistoryTimeline` and `MarketStatusTimeline`.
 *
 * Both dates are shown for the same reason the domain keeps both: a question
 * answered in March and entered in August is not the same record as one
 * answered in August, and a reader who cannot see both cannot tell.
 */
export function QuestionHistoryTimeline({
  history,
}: QuestionHistoryTimelineProps) {
  if (history.length === 0) return null;

  return (
    <ol className="mt-3 space-y-2 border-l pl-4" data-testid="question-history">
      {history.map((entry, index) => (
        <li key={`${entry.status}-${entry.occurredOn}-${index}`} className="text-sm">
          <span className="font-medium">{entry.status}</span>
          <span className="text-muted-foreground"> · {entry.occurredOn}</span>
          <span className="text-muted-foreground">
            {" "}
            (recorded {entry.recordedOnUtc.slice(0, 10)})
          </span>
          {entry.note && (
            <p className="text-muted-foreground">{entry.note}</p>
          )}
        </li>
      ))}
    </ol>
  );
}
