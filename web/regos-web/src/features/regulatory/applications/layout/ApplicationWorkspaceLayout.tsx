import { Outlet } from "react-router-dom";

import { ApplicationWorkspaceHeader } from "../components/ApplicationWorkspaceHeader";
import { ApplicationWorkspaceNavigation } from "./ApplicationWorkspaceNavigation";

export function ApplicationWorkspaceLayout() {
  return (
    <div className="flex h-full flex-col">
      <ApplicationWorkspaceHeader />

      <div className="flex flex-1 overflow-hidden">
        <aside className="w-64 border-r bg-muted/20">
          <ApplicationWorkspaceNavigation />
        </aside>

        <main className="flex-1 overflow-auto">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
