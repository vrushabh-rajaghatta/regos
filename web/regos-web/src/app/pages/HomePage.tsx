import { Navigate } from "react-router-dom";

import { useCurrentUser } from "@/features/auth/hooks/useCurrentUser";

export function HomePage() {
  const { data: user } = useCurrentUser();

  // A platform administrator has no tenant, so every tenant-scoped screen
  // would greet them with errors; their home is tenant administration.
  if (user?.role === "PlatformAdministrator") {
    return <Navigate to="/platform/tenants" replace />;
  }

  return (
    <div>
      <h1 className="text-3xl font-semibold">Welcome to RegOS</h1>

      <p className="mt-2 text-muted-foreground">
        Enterprise Life Sciences Platform
      </p>
    </div>
  );
}
