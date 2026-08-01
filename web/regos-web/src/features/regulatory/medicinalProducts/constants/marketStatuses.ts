/**
 * The commercial states a market presence can hold.
 *
 * A fixed reference list (SC-105), mirroring the `MarketStatus` enum. Unlike a
 * registration's lifecycle, the server offers no "what may follow this" answer,
 * because there is no transition table to ask — commercial reality permits
 * almost any sequence. The one exception is `Planned`, which a market that has
 * been entered cannot return to, so it is never offered as a choice.
 */
export const MARKET_STATUSES = [
  { value: "Launched", label: "Launched" },
  { value: "TemporarilyUnavailable", label: "Temporarily unavailable" },
  { value: "Discontinued", label: "Discontinued" },
] as const;

/** Includes the initial state, which is displayed but never chosen. */
export function marketStatusLabel(status: string): string {
  if (status === "Planned") return "Planned";

  return (
    MARKET_STATUSES.find((option) => option.value === status)?.label ?? status
  );
}
