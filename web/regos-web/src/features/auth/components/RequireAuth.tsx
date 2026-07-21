import { Navigate, Outlet, useLocation } from "react-router-dom";

import { getAccessToken } from "@/shared/auth/accessToken";

/**
 * Keeps unauthenticated visitors out of the application shell.
 *
 * This is a routing convenience, not a security boundary. It checks only that a
 * token exists — not that it is valid, unexpired, or issued by us. Forging one
 * in local storage gets you a rendered page whose every API call returns 401.
 * Authorization is decided by the API; this exists so that the common case
 * lands on a sign-in form rather than on a screen full of failed requests.
 */
export function RequireAuth() {
  const location = useLocation();

  if (getAccessToken()) {
    return <Outlet />;
  }

  // Remember where they were headed so signing in resumes it rather than
  // dumping them on the home page.
  return <Navigate to="/login" state={{ from: location }} replace />;
}
