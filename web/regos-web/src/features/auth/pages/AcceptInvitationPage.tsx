import { useSearchParams } from "react-router-dom";

import { AcceptInvitationForm } from "../components/AcceptInvitationForm";

export function AcceptInvitationPage() {
  const [params] = useSearchParams();

  const token = params.get("token");

  return (
    <div className="min-h-screen flex items-center justify-center p-6">
      <div className="w-full max-w-sm space-y-8">
        <div>
          <h1 className="text-3xl font-semibold">Welcome to RegOS</h1>

          <p className="mt-2 text-muted-foreground">
            Choose a password to finish setting up your account.
          </p>
        </div>

        {/* A link with no token cannot be salvaged, and showing the form would
            only let someone fill it in and be refused. */}
        {token ? (
          <AcceptInvitationForm token={token} />
        ) : (
          <p className="text-sm text-destructive" role="alert">
            This invitation link is incomplete. Please use the link from your
            invitation email, or ask for a new one.
          </p>
        )}
      </div>
    </div>
  );
}
