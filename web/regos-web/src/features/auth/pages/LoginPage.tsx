import { useLocation, useNavigate, type Location } from "react-router-dom";

import { getAccessToken } from "@/shared/auth/accessToken";

import { LoginForm } from "../components/LoginForm";

export function LoginPage() {
  const navigate = useNavigate();
  const location = useLocation();

  const from = (location.state as { from?: Location } | null)?.from?.pathname;

  function onSuccess() {
    navigate(from ?? "/", { replace: true });
  }

  return (
    <div className="min-h-screen flex items-center justify-center p-6">
      <div className="w-full max-w-sm space-y-8">
        <div>
          <h1 className="text-3xl font-semibold">RegOS</h1>

          <p className="mt-2 text-muted-foreground">
            Sign in to continue.
          </p>
        </div>

        <LoginForm onSuccess={onSuccess} />

        {getAccessToken() && (
          <p className="text-sm text-muted-foreground" role="status">
            You are already signed in. Signing in again replaces the current
            session.
          </p>
        )}
      </div>
    </div>
  );
}
