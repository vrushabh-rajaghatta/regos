import { RegulatoryNavigation } from "./RegulatoryNavigation";
import { Outlet } from "react-router-dom";

export function RegulatoryLayout() {
  return (
    <div className="flex h-full">
      <RegulatoryNavigation />

      <section className="flex-1 p-6">
        <Outlet />
      </section>
    </div>
  );
}
