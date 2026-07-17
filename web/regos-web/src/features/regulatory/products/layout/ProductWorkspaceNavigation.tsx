import { NavLink, useParams } from "react-router-dom";

export function ProductWorkspaceNavigation() {
  const { productId } = useParams();

  const linkClass = ({ isActive }: { isActive: boolean }) =>
    `block rounded-md px-3 py-2 text-sm ${
      isActive
        ? "bg-primary text-primary-foreground"
        : "text-muted-foreground hover:bg-muted"
    }`;

  const comingSoon = ["Publishing", "History"];

  return (
    <nav className="space-y-1 p-4">
      <NavLink
        end
        to={`/regulatory/products/${productId}`}
        className={linkClass}
      >
        Overview
      </NavLink>

      <NavLink
        to={`/regulatory/products/${productId}/applications`}
        className={linkClass}
      >
        Applications
      </NavLink>

      <NavLink
        to={`/regulatory/products/${productId}/documents`}
        className={linkClass}
      >
        Documents
      </NavLink>

      {comingSoon.map((label) => (
        <div
          key={label}
          aria-disabled
          title="Coming soon"
          className="flex cursor-default items-center justify-between rounded-md px-3 py-2 text-sm text-muted-foreground/60"
        >
          <span>{label}</span>
          <span className="text-[10px] uppercase tracking-wide">
            Coming soon
          </span>
        </div>
      ))}
    </nav>
  );
}
