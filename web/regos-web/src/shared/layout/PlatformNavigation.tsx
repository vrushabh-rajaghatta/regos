import { PlatformNavigationItem } from "./PlatformNavigationItem";

const modules = [
  "Dashboard",
  "Regulatory",
  "Quality",
  "Clinical",
  "CMC",
  "Safety",
  "Administration",
];

export function PlatformNavigation() {
  return (
    <aside className="w-64 border-r p-3">
      {modules.map((module) => (
        <PlatformNavigationItem key={module} title={module} />
      ))}
    </aside>
  );
}
