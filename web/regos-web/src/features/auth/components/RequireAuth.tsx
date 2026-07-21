import { Navigate, Outlet, useLocation } from "react-router-dom";

import { useCurrentUser } from "../hooks/useCurrentUser";

/**
 * Keeps unauthenticated visitors out of the application shell.
 *
 * It asks the API who we are rather than inspecting a stored token, because
 * there is no longer a token to inspect — the session is HttpOnly cookies
 * (ADR-025). That makes this stricter than the version it replaced: that one
 * trusted the mere presence of a value in local storage, and a forged one got
 * you a rendered page full of failing requests. This cannot be satisfied by
 * anything a script can write.
 *
 * It is still not the security boundary. Authorization is decided by the API on
 * every request; this only decides what to render.
 */
export function RequireAuth() {
  const location = useLocation();

  const { data, isPending } = useCurrentUser();

  // The first paint of a reload has no answer yet. Rendering the shell here
  // would flash the app at someone who turns out to be signed out; redirecting
  // would throw out a perfectly good session.
  if (isPending) return null;

  if (data) return <Outlet />;

  // Remember where they were headed so signing in resumes it rather than
  // dumping them on the home page.
  return <Navigate to="/login" state={{ from: location }} replace />;
}
