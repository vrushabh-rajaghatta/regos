import { NavLink } from "react-router-dom";

export function SubmissionWorkspaceNavigation() {
  const linkClass = ({ isActive }: { isActive: boolean }) =>
    `block rounded-md px-3 py-2 text-sm ${
      isActive
        ? "bg-primary text-primary-foreground"
        : "text-muted-foreground hover:bg-muted"
    }`;

  // No "Submissions" item — a Submission is already the current context.
  return (
    <nav className="space-y-1 p-4">
      <NavLink end to="" className={linkClass}>
        Overview
      </NavLink>

      {/*
        Documents is the dossier's inventory — what is attached. Content Plan is
        its structure — where each of those sits, and what is still expected.
        Two questions, so two pages.
      */}
      <NavLink to="documents" className={linkClass}>
        Documents
      </NavLink>

      <NavLink to="content-plan" className={linkClass}>
        Content Plan
      </NavLink>

      <NavLink to="validation" className={linkClass}>
        Validation
      </NavLink>

      <NavLink to="publishing" className={linkClass}>
        Publishing
      </NavLink>

      {/* What this filing did to the sequence before it — frozen at publish,
          so it is a record rather than a live comparison. */}
      <NavLink to="changes" className={linkClass}>
        What changed
      </NavLink>

      <NavLink to="history" className={linkClass}>
        History
      </NavLink>
    </nav>
  );
}
