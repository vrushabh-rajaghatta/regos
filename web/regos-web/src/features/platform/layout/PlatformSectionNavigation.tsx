import { NavLink } from "react-router-dom";

const items = [
  { label: "Organizations", to: "/platform/organizations" },
  { label: "Users", to: "/platform/users" },
];

export function PlatformSectionNavigation() {
  return (
    <nav className="w-60 border-r p-3">
      {items.map((item) => (
        <NavLink
          key={item.to}
          to={item.to}
          className="block rounded-md px-3 py-2 hover:bg-muted"
        >
          {item.label}
        </NavLink>
      ))}
    </nav>
  );
}
