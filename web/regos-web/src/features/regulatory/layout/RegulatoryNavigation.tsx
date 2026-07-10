import { NavLink } from "react-router-dom";

const items = ["Products", "Submissions", "Authorities", "Templates"];

export function RegulatoryNavigation() {
  return (
    <nav className="w-60 border-r p-3">
      {items.map((item) => (
        <NavLink
          to={`/regulatory/${item.toLowerCase()}`}
          className="block rounded-md px-3 py-2 hover:bg-muted"
        >
          {item}
        </NavLink>
      ))}
    </nav>
  );
}
