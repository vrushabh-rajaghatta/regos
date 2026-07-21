import { Link, useSearchParams } from "react-router-dom";

import { ResetPasswordForm } from "../components/ResetPasswordForm";

export function ResetPasswordPage() {
  const [params] = useSearchParams();

  const token = params.get("token");

  return (
    <div className="min-h-screen flex items-center justify-center p-6">
      <div className="w-full max-w-sm space-y-8">
        <div>
          <h1 className="text-3xl font-semibold">Choose a new password</h1>

          <p className="mt-2 text-muted-foreground">
            You will sign in with this password from now on.
          </p>
        </div>

        {/* A link with no token cannot be salvaged, and showing the form would
            only let someone fill it in and be refused. */}
        {token ? (
          <ResetPasswordForm token={token} />
        ) : (
          <p className="text-sm text-destructive" role="alert">
            This reset link is incomplete. Please use the link from your email,
            or{" "}
            <Link to="/forgot-password" className="underline">
              request a new one
            </Link>
            .
          </p>
        )}
      </div>
    </div>
  );
}
