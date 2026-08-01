import { NavLink } from "react-router-dom";

const items = [
  "Due work",
  "Products",
  "Registrations",
  "Correspondence",
  "Submissions",
  "Organizations",
  "Sites",
  "Contacts",
  "Authorities",
  "Templates",
];

export function RegulatoryNavigation() {
  return (
    <nav className="w-60 border-r p-3">
      {items.map((item) => (
        <NavLink
          key={item}
          to={`/regulatory/${item.toLowerCase().replace(/ /g, "-")}`}
          className="block rounded-md px-3 py-2 hover:bg-muted"
        >
          {item}
        </NavLink>
      ))}
    </nav>
  );
}
