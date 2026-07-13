import { NavLink, useParams } from "react-router-dom";

export function ProductWorkspaceNavigation() {
  const { productId } = useParams();

  const linkClass = ({ isActive }: { isActive: boolean }) =>
    `block rounded-md px-3 py-2 text-sm ${
      isActive
        ? "bg-primary text-primary-foreground"
        : "text-muted-foreground hover:bg-muted"
    }`;

  return (
    <nav className="p-4 space-y-2">
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

      <div className="rounded-md px-3 py-2 text-sm text-muted-foreground">
        History
      </div>
    </nav>
  );
}
