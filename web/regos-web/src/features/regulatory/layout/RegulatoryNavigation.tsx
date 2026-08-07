import { NavLink } from "react-router-dom";

const items = [
  { label: "Due work", path: "due-work" },
  { label: "Products", path: "products" },
  { label: "Substances", path: "substances" },
  { label: "Registrations", path: "registrations" },
  { label: "Studies", path: "studies" },
  { label: "Correspondence", path: "correspondence" },
  { label: "Meetings", path: "meetings" },
  { label: "Inspections", path: "inspections" },
  { label: "Submissions", path: "submissions" },
  { label: "Organizations", path: "organizations" },
  { label: "Sites", path: "sites" },
  { label: "Contacts", path: "contacts" },
  { label: "Authorities", path: "authorities" },
  { label: "Templates", path: "templates" },
  { label: "Playbooks", path: "playbooks" },
  { label: "Objectives", path: "objectives" },
  { label: "Plan board", path: "plan-board" },
];

const linkClass = ({ isActive }: { isActive: boolean }) =>
  `block rounded-md px-3 py-2 ${
    isActive
      ? "bg-primary text-primary-foreground"
      : "text-muted-foreground hover:bg-muted"
  }`;

export function RegulatoryNavigation() {
  return (
    <nav className="w-60 border-r p-3">
      {items.map((item) => (
        <NavLink
          key={item.label}
          to={`/regulatory/${item.path}`}
          className={linkClass}
        >
          {item.label}
        </NavLink>
      ))}
    </nav>
  );
}
