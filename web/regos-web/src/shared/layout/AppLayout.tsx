import { Header } from "./Header";
import { PlatformNavigation } from "./PlatformNavigation";
import { Outlet } from "react-router-dom";

export function AppLayout() {
  return (
    <div className="h-screen flex flex-col">
      <Header />

      <div className="flex flex-1 overflow-hidden">
        <PlatformNavigation />

        <main className="flex-1 overflow-auto p-6">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
