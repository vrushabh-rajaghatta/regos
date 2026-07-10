interface SidebarItemProps {
  title: string;
}

export function PlatformNavigationItem({ title }: SidebarItemProps) {
  return (
    <button
      className="
                w-full
                rounded-md
                px-3
                py-2
                text-left
                text-sm
                hover:bg-muted
            "
    >
      {title}
    </button>
  );
}
