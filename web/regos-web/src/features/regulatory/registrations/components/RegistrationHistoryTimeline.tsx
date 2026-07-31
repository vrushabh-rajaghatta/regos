import { RegistrationStatusBadge } from "./RegistrationStatusBadge";
import type { RegistrationStatusEntry } from "../types/RegistrationDetail";

/**
 * The record a regulator would read: every status held, oldest first, each
 * carrying both of its dates.
 *
 * The two are never conflated. "Occurred" is when it happened in the world;
 * "recorded" is when RegOS learned of it. A migrated authorisation shows a 2019
 * approval recorded today, and that difference is the point.
 */
export function RegistrationHistoryTimeline({
  history,
}: {
  history: RegistrationStatusEntry[];
}) {
  if (history.length === 0) {
    return <p className="text-sm text-muted-foreground">No history yet.</p>;
  }

  return (
    <ol className="space-y-4" data-testid="registration-history">
      {history.map((entry) => (
        <li
          key={entry.id}
          className="border-l-2 pl-4"
          data-testid="registration-history-entry"
        >
          <div className="flex flex-wrap items-center gap-2">
            <RegistrationStatusBadge status={entry.status} />

            <span className="text-sm font-medium">
              {new Date(entry.occurredOn).toLocaleDateString()}
            </span>

            <span className="text-xs text-muted-foreground">
              recorded {new Date(entry.recordedOnUtc).toLocaleDateString()}
            </span>
          </div>

          {entry.note && (
            <p className="mt-1 text-sm text-muted-foreground">{entry.note}</p>
          )}
        </li>
      ))}
    </ol>
  );
}
