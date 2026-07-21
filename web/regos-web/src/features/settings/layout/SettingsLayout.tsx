import { Outlet } from "react-router-dom";

import { SettingsSectionNavigation } from "./SettingsSectionNavigation";

export function SettingsLayout() {
  return (
    <div className="flex h-full">
      <SettingsSectionNavigation />

      <section className="flex-1 p-6">
        <Outlet />
      </section>
    </div>
  );
}
