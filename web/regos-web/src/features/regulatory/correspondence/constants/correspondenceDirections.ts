/**
 * The two directions, with the words a regulatory user would say. "Received"
 * and "Sent" read better on a screen than the domain's Inbound/Outbound, which
 * stay in the API and the type — the domain's word and the screen's word may
 * differ, and both are binding.
 */
export const CORRESPONDENCE_DIRECTIONS = [
  { value: "Inbound", label: "Received" },
  { value: "Outbound", label: "Sent" },
] as const;

export function directionLabel(direction: string): string {
  return (
    CORRESPONDENCE_DIRECTIONS.find((d) => d.value === direction)?.label ??
    direction
  );
}
