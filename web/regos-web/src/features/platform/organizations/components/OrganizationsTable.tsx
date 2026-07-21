import type { OrganizationListItem } from "../types/OrganizationListItem";
import { organizationTypeLabel } from "../types/OrganizationType";
import { OrganizationStatusBadge } from "./OrganizationStatusBadge";

interface OrganizationsTableProps {
  organizations: OrganizationListItem[];
}

export function OrganizationsTable({
  organizations,
}: OrganizationsTableProps) {
  return (
    <div className="overflow-x-auto rounded-lg border">
      <table className="w-full text-sm" data-testid="organization-list">
        <thead className="border-b bg-muted/50">
          <tr className="text-left">
            <th className="px-4 py-2.5 font-medium">Legal Name</th>
            <th className="px-4 py-2.5 font-medium">Type</th>
            <th className="px-4 py-2.5 font-medium">Status</th>
          </tr>
        </thead>

        <tbody>
          {organizations.map((organization) => (
            <tr
              key={organization.id}
              className="border-b last:border-0 hover:bg-muted/40"
            >
              <td className="px-4 py-2.5 font-medium">
                {organization.legalName}
              </td>

              <td className="px-4 py-2.5 text-muted-foreground">
                {organizationTypeLabel(organization.type)}
              </td>

              <td className="px-4 py-2.5" data-testid="organization-status">
                <OrganizationStatusBadge status={organization.status} />
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
