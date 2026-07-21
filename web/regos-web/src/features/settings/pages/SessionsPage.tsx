import { ActiveSessions } from "../components/ActiveSessions";
import { PageHeader } from "@/shared/components/PageHeader";

export function SessionsPage() {
  return (
    <div className="space-y-8">
      <PageHeader
        title="Active Sessions"
        description="Where you are signed in. Ending a session signs that device out immediately."
      />

      <ActiveSessions />
    </div>
  );
}
