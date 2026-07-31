import { Navigate } from "react-router-dom";

import { useCurrentUser } from "@/features/auth/hooks/useCurrentUser";

/**
 * The platform section's landing depends on who arrived (ADR-033). A tenant
 * administrator used to land on Organizations; those moved to /regulatory in
 * EPIC-016 S004, leaving Users as the only platform page they can open.
 */
export function PlatformIndexRedirect() {
  const { data: user, isPending } = useCurrentUser();

  if (isPending) return null;

  return user?.role === "PlatformAdministrator" ? (
    <Navigate to="tenants" replace />
  ) : (
    <Navigate to="users" replace />
  );
}
