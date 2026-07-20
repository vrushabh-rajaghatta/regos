import { NavLink } from "react-router-dom";

interface SidebarItemProps {
  title: string;
  to?: string;
}

const itemClassName = `
  block
  w-full
  rounded-md
  px-3
  py-2
  text-left
  text-sm
  hover:bg-muted
`;

export function PlatformNavigationItem({ title, to }: SidebarItemProps) {
  if (to) {
    return (
      <NavLink to={to} className={itemClassName}>
        {title}
      </NavLink>
    );
  }

  return <button className={itemClassName}>{title}</button>;
}
