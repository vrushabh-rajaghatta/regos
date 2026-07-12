import { Outlet } from "react-router-dom";
import { ProductWorkspaceNavigation } from "./ProductWorkspaceNavigation";

export function ProductWorkspaceLayout() {
  return (
    <div className="flex h-full">
      <aside className="w-64 border-r bg-muted/20">
        <ProductWorkspaceNavigation />
      </aside>

      <main className="flex-1 overflow-auto">
        <Outlet />
      </main>
    </div>
  );
}
