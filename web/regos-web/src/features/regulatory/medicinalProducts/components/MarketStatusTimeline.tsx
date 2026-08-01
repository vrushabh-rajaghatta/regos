import { marketStatusLabel } from "../constants/marketStatuses";
import type { MarketStatusEntry } from "../types/MedicinalProductDetail";

/**
 * The commercial record of a market: every status held, oldest first, each
 * carrying both of its dates.
 *
 * Mirrors `RegistrationHistoryTimeline` on purpose — the two histories are the
 * same shape, and a reader who has learned one should not have to learn the
 * other. "Occurred" is when it became true in the market; "recorded" is when
 * RegOS learned of it, and a launch backdated to 2021 and entered today shows
 * both. Until this page existed, the second timestamp was stored and never
 * shown, which made it invisible complexity.
 */
export function MarketStatusTimeline({
  history,
}: {
  history: MarketStatusEntry[];
}) {
  if (history.length === 0) {
    return <p className="text-sm text-muted-foreground">No history yet.</p>;
  }

  return (
    <ol className="space-y-4" data-testid="market-status-history">
      {history.map((entry) => (
        <li
          key={entry.id}
          className="border-l-2 pl-4"
          data-testid="market-status-history-entry"
        >
          <div className="flex flex-wrap items-center gap-2">
            <span className="rounded bg-muted px-1.5 py-0.5 text-xs font-medium">
              {marketStatusLabel(entry.status)}
            </span>

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
