import { Badge } from "@/components/ui/badge";

import type { ContactRole } from "../types/Contact";

export function ContactRoleBadges({ roles }: { roles: ContactRole[] }) {
  if (roles.length === 0) return null;

  return (
    <div className="flex flex-wrap gap-1">
      {roles.map((role) => (
        <Badge key={role.roleId} variant="secondary" title={role.code}>
          {role.name}
        </Badge>
      ))}
    </div>
  );
}
