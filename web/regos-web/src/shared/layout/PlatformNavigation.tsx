import { PlatformNavigationItem } from "./PlatformNavigationItem";

// `to` links a module to its landing route; modules without a route yet remain
// non-navigating placeholders.
const modules: { title: string; to?: string }[] = [
  { title: "Dashboard" },
  { title: "Regulatory", to: "/regulatory/products" },
  { title: "Quality" },
  { title: "Clinical" },
  { title: "CMC" },
  { title: "Safety" },
  { title: "Administration", to: "/platform/users" },
  { title: "Settings", to: "/settings/security" },
];

export function PlatformNavigation() {
  return (
    <aside className="w-64 border-r p-3">
      {modules.map((module) => (
        <PlatformNavigationItem
          key={module.title}
          title={module.title}
          to={module.to}
        />
      ))}
    </aside>
  );
}
