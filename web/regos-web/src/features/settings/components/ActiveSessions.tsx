import { useQueryClient } from "@tanstack/react-query";
import { useNavigate } from "react-router-dom";

import { Button } from "@/components/ui/button";
import {
  useRevokeOtherSessions,
  useRevokeSession,
  useSessions,
} from "@/features/auth/hooks/useSessions";
import type { SessionSummary } from "@/features/auth/types/SessionSummary";

function formatWhen(value: string): string {
  return new Date(value).toLocaleString();
}

export function ActiveSessions() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  const { data: sessions, isPending, isError } = useSessions();
  const revoke = useRevokeSession();
  const revokeOthers = useRevokeOtherSessions();

  function endSession(session: SessionSummary) {
    revoke.mutate(session.id, {
      onSuccess: () => {
        // Ending your own current session is signing yourself out: the server
        // has already cleared the cookies, so staying here would render a shell
        // backed by a session that no longer exists - and refetching the list
        // would only produce a 401.
        if (session.isCurrent) {
          queryClient.clear();

          navigate("/login", { replace: true });

          return;
        }

        queryClient.invalidateQueries({ queryKey: ["sessions"] });
      },
    });
  }

  if (isPending) return <p className="text-sm text-muted-foreground">Loading…</p>;

  if (isError) {
    return (
      <p className="text-sm text-destructive" role="alert">
        Unable to load your sessions.
      </p>
    );
  }

  const others = sessions.filter((session) => !session.isCurrent);

  return (
    <div className="space-y-4" data-testid="active-sessions">
      <ul className="divide-y rounded-md border">
        {sessions.map((session) => (
          <li
            key={session.id}
            className="flex items-start justify-between gap-4 p-4"
            data-testid="session-row"
          >
            <div className="min-w-0 space-y-1">
              <p className="text-sm font-medium">
                {session.isCurrent ? "This device" : "Signed in"}
              </p>

              {/* Raw, unparsed. RegOS does not guess which browser this is,
                  because a confident wrong guess is worse than a long string
                  the owner recognises (ADR-029). */}
              <p className="text-xs text-muted-foreground break-all">
                {session.userAgent ?? "Unknown device"}
              </p>

              <p className="text-xs text-muted-foreground">
                {session.createdFromIp ? `${session.createdFromIp} · ` : ""}
                started {formatWhen(session.createdOn)} · last used{" "}
                {formatWhen(session.lastUsedOn)}
              </p>
            </div>

            <Button
              variant="ghost"
              size="sm"
              onClick={() => endSession(session)}
              disabled={revoke.isPending}
            >
              {session.isCurrent ? "Sign out" : "End session"}
            </Button>
          </li>
        ))}
      </ul>

      {others.length > 0 && (
        <Button
          variant="outline"
          onClick={() => revokeOthers.mutate()}
          disabled={revokeOthers.isPending}
        >
          {revokeOthers.isPending
            ? "Ending…"
            : `Sign out ${others.length} other ${
                others.length === 1 ? "session" : "sessions"
              }`}
        </Button>
      )}

      {(revoke.isError || revokeOthers.isError) && (
        <p className="text-sm text-destructive" role="alert">
          {(revoke.error ?? revokeOthers.error)?.message}
        </p>
      )}
    </div>
  );
}
