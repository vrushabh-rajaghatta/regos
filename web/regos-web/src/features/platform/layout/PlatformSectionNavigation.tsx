import { NavLink } from "react-router-dom";

import { useCurrentUser } from "@/features/auth/hooks/useCurrentUser";

/**
 * Role-aware (ADR-033): a platform administrator manages tenants and has no
 * tenant of their own to manage users in; a tenant administrator manages users
 * and cannot see the tenant directory. The API enforces this with 403s —
 * hiding the links just keeps the UI from offering doors that will not open.
 *
 * Organizations are not here. They are regulatory parties (ADR-030, ADR-032),
 * and live under /regulatory beside Products and Registrations.
 */
export function PlatformSectionNavigation() {
  const { data: user } = useCurrentUser();

  const items =
    user?.role === "PlatformAdministrator"
      ? [{ label: "Tenants", to: "/platform/tenants" }]
      : [{ label: "Users", to: "/platform/users" }];

  return (
    <nav className="w-60 border-r p-3">
      {items.map((item) => (
        <NavLink
          key={item.to}
          to={item.to}
          className="block rounded-md px-3 py-2 hover:bg-muted"
        >
          {item.label}
        </NavLink>
      ))}
    </nav>
  );
}
