import { Outlet } from "react-router-dom";

import { DocumentWorkspaceHeader } from "../components/DocumentWorkspaceHeader";
import { DocumentWorkspaceNavigation } from "./DocumentWorkspaceNavigation";

export function DocumentWorkspaceLayout() {
  return (
    <div className="flex h-full flex-col">
      <DocumentWorkspaceHeader />

      <div className="flex flex-1 overflow-hidden">
        <aside className="w-64 border-r bg-muted/20">
          <DocumentWorkspaceNavigation />
        </aside>

        <main className="flex-1 overflow-auto">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
