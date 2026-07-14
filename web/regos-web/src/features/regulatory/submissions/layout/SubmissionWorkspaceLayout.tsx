import { Outlet } from "react-router-dom";

import { SubmissionWorkspaceHeader } from "../components/SubmissionWorkspaceHeader";
import { SubmissionWorkspaceNavigation } from "./SubmissionWorkspaceNavigation";

export function SubmissionWorkspaceLayout() {
  return (
    <div className="flex h-full flex-col">
      <SubmissionWorkspaceHeader />

      <div className="flex flex-1 overflow-hidden">
        <aside className="w-64 border-r bg-muted/20">
          <SubmissionWorkspaceNavigation />
        </aside>

        <main className="flex-1 overflow-auto">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
