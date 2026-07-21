import { useEffect } from "react";
import { useLocation, useNavigate, type Location } from "react-router-dom";

import { useCurrentUser } from "../hooks/useCurrentUser";
import { LoginForm } from "../components/LoginForm";

export function LoginPage() {
  const navigate = useNavigate();
  const location = useLocation();

  const from = (location.state as { from?: Location } | null)?.from?.pathname;

  const destination = from ?? "/";

  // Someone with a live session has no business on the sign-in page — most
  // often it is a bookmark, or the back button after signing in.
  const { data: currentUser } = useCurrentUser();

  useEffect(() => {
    if (currentUser) navigate(destination, { replace: true });
  }, [currentUser, destination, navigate]);

  return (
    <div className="min-h-screen flex items-center justify-center p-6">
      <div className="w-full max-w-sm space-y-8">
        <div>
          <h1 className="text-3xl font-semibold">RegOS</h1>

          <p className="mt-2 text-muted-foreground">Sign in to continue.</p>
        </div>

        <LoginForm onSuccess={() => navigate(destination, { replace: true })} />
      </div>
    </div>
  );
}
