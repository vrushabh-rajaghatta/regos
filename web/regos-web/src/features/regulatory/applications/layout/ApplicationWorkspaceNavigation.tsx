import { NavLink } from "react-router-dom";

export function ApplicationWorkspaceNavigation() {
  const linkClass = ({ isActive }: { isActive: boolean }) =>
    `block rounded-md px-3 py-2 text-sm ${
      isActive
        ? "bg-primary text-primary-foreground"
        : "text-muted-foreground hover:bg-muted"
    }`;

  return (
    <nav className="space-y-1 p-4">
      <NavLink end to="" className={linkClass}>
        Overview
      </NavLink>

      <NavLink to="submissions" className={linkClass}>
        Submissions
      </NavLink>

      <NavLink to="documents" className={linkClass}>
        Documents
      </NavLink>

      <NavLink to="publishing" className={linkClass}>
        Publishing
      </NavLink>

      <NavLink to="history" className={linkClass}>
        History
      </NavLink>
    </nav>
  );
}
