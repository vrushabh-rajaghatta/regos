import { useMemo } from "react";

import { statusLabel } from "./statusLabel";

interface Props {
  statuses: string[];
  value: string;
  onChange: (value: string) => void;
}

/**
 * Narrows a portfolio view without hiding anything the server sent.
 *
 * The options are derived from the rows on screen rather than from a list of
 * statuses held here — so this knows nothing about the lifecycle, not even
 * which statuses are terminal. Deciding that "active" means one set of statuses
 * rather than another would be exactly the domain policy STORY-002 kept on the
 * server.
 */
export function RegistrationStatusFilter({ statuses, value, onChange }: Props) {
  const options = useMemo(
    () => [...new Set(statuses)].sort(),
    [statuses]
  );

  if (options.length < 2) return null;

  return (
    <label className="flex items-center gap-2 text-sm">
      <span className="text-muted-foreground">Status</span>

      <select
        aria-label="Filter by status"
        value={value}
        onChange={(event) => onChange(event.target.value)}
        data-testid="registration-status-filter"
        className="h-9 rounded-md border bg-transparent px-3 text-sm"
      >
        <option value="">All</option>
        {options.map((status) => (
          <option key={status} value={status}>
            {statusLabel(status)}
          </option>
        ))}
      </select>
    </label>
  );
}
