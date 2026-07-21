import { NavLink } from "react-router-dom";

// Active sessions joins this list in AUTH-010. The section exists with one item
// because the item needs somewhere to live, not because one item needs a menu.
const items = [{ label: "Security", to: "/settings/security" }];

export function SettingsSectionNavigation() {
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
