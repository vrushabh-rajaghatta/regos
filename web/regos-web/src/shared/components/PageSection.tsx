import type { ReactNode } from "react";

interface PageSectionProps {
  title?: string;
  children: ReactNode;
}

export function PageSection({ title, children }: PageSectionProps) {
  return (
    <section className="space-y-4">
      {title && <h2 className="text-lg font-medium">{title}</h2>}

      {children}
    </section>
  );
}
