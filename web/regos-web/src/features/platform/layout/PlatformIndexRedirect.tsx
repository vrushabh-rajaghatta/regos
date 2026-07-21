import { Navigate } from "react-router-dom";

import { useCurrentUser } from "@/features/auth/hooks/useCurrentUser";

/** The platform section's landing depends on who arrived (ADR-033). */
export function PlatformIndexRedirect() {
  const { data: user, isPending } = useCurrentUser();

  if (isPending) return null;

  return user?.role === "PlatformAdministrator" ? (
    <Navigate to="tenants" replace />
  ) : (
    <Navigate to="organizations" replace />
  );
}
