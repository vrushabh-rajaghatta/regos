import { Outlet } from "react-router-dom";

import { PlatformSectionNavigation } from "./PlatformSectionNavigation";

export function PlatformLayout() {
  return (
    <div className="flex h-full">
      <PlatformSectionNavigation />

      <section className="flex-1 p-6">
        <Outlet />
      </section>
    </div>
  );
}
