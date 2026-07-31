import { NavLink, useParams } from "react-router-dom";

/**
 * The first workspace sidebar whose subject is a company rather than a product.
 * Same shape as ProductWorkspaceNavigation on purpose: an organization is not a
 * product, but "look at one thing from four angles" is the same interaction,
 * and reusing it costs nothing while inventing a second one costs a user their
 * bearings.
 */
export function OrganizationWorkspaceNavigation() {
  const { organizationId } = useParams();

  const linkClass = ({ isActive }: { isActive: boolean }) =>
    `block rounded-md px-3 py-2 text-sm ${
      isActive
        ? "bg-primary text-primary-foreground"
        : "text-muted-foreground hover:bg-muted"
    }`;

  const base = `/regulatory/organizations/${organizationId}`;

  return (
    <nav className="space-y-1 p-4" data-testid="organization-workspace-nav">
      <NavLink end to={base} className={linkClass}>
        Overview
      </NavLink>

      <NavLink to={`${base}/divisions`} className={linkClass}>
        Divisions
      </NavLink>

      <NavLink to={`${base}/sites`} className={linkClass}>
        Sites
      </NavLink>

      <NavLink to={`${base}/contacts`} className={linkClass}>
        Contacts
      </NavLink>
    </nav>
  );
}
