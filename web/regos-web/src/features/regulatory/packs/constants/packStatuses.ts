import type { PackageMarketingStatus } from "../types/Pack";

/**
 * Every option is always offered, because there is no transition table — a pack
 * may be discontinued and reintroduced. `Planned` is absent rather than
 * disabled: it is the state a pack begins in, and one that reached the market
 * cannot be intended again.
 */
export const PACK_STATUSES: {
  value: Exclude<PackageMarketingStatus, "Planned">;
  label: string;
}[] = [
  { value: "Marketed", label: "On sale" },
  { value: "TemporarilyUnavailable", label: "Temporarily unavailable" },
  { value: "Discontinued", label: "Discontinued" },
];

export function packStatusLabel(status: PackageMarketingStatus): string {
  return status === "Planned"
    ? "Planned"
    : (PACK_STATUSES.find((x) => x.value === status)?.label ?? status);
}
