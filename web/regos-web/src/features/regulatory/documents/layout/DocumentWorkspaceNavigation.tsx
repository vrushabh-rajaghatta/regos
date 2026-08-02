import { NavLink } from "react-router-dom";

export function DocumentWorkspaceNavigation() {
  const linkClass = ({ isActive }: { isActive: boolean }) =>
    `block rounded-md px-3 py-2 text-sm ${
      isActive
        ? "bg-primary text-primary-foreground"
        : "text-muted-foreground hover:bg-muted"
    }`;

  // Only Overview is functional today; the rest are reserved so the
  // workspace shape is stable as capabilities land.
  return (
    <nav className="space-y-1 p-4">
      <NavLink to="overview" className={linkClass}>
        Overview
      </NavLink>

      <NavLink to="versions" className={linkClass}>
        Versions
      </NavLink>

      {/* Not "History": that is this document's own audit trail. This is
          where it went — the sequences that placed or withdrew it. */}
      <NavLink to="usage" className={linkClass}>
        In filings
      </NavLink>

      <NavLink to="history" className={linkClass}>
        History
      </NavLink>

      <NavLink to="ai-insights" className={linkClass}>
        AI Insights
      </NavLink>
    </nav>
  );
}
