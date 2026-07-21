import { Button } from "@/components/ui/button";
import { useCurrentUser } from "@/features/auth/hooks/useCurrentUser";
import { useSignOut } from "@/features/auth/hooks/useSignOut";

export function Header() {
  // The email comes from the API, not from decoding the token in the browser.
  // Reading claims client-side would mean trusting a string we never verified;
  // asking the server means the answer is whoever the signature says it is.
  const { data: currentUser } = useCurrentUser();

  const signOut = useSignOut();

  return (
    <header className="h-14 border-b flex items-center justify-between px-6 bg-background">
      <h1 className="text-lg font-semibold">RegOS</h1>

      <div className="flex items-center gap-4">
        <span className="text-sm text-muted-foreground">
          {currentUser?.email ?? ""}
        </span>

        <Button
          variant="ghost"
          size="sm"
          onClick={() => signOut.mutate()}
          disabled={signOut.isPending}
        >
          Sign Out
        </Button>
      </div>
    </header>
  );
}
