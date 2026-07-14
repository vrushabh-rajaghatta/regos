import { useParams } from "react-router-dom";

import { useApplication } from "../hooks/useApplication";
import { ApplicationStatusBadge } from "./ApplicationStatusBadge";

export function ApplicationWorkspaceHeader() {
  const { applicationId } = useParams();

  // Shares the Overview page's query; React Query serves the cached
  // result, so this does not trigger an additional network request.
  const { data: application } = useApplication(applicationId!);

  if (!application) {
    return null;
  }

  return (
    <header className="border-b px-6 py-4">
      <div className="flex items-start justify-between gap-4">
        <div className="space-y-1">
          <h1 className="text-xl font-semibold">{application.name}</h1>

          <p className="text-sm text-muted-foreground">
            {application.countryName} • {application.authorityName}
          </p>
        </div>

        <ApplicationStatusBadge status={application.status} />
      </div>
    </header>
  );
}
