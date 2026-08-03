import { useState } from "react";
import { useParams } from "react-router-dom";

import { Button } from "@/components/ui/button";

import { useApplication } from "../hooks/useApplication";
import { ApplicationStatusBadge } from "./ApplicationStatusBadge";
import { RecordApplicationNumberDialog } from "./RecordApplicationNumberDialog";

export function ApplicationWorkspaceHeader() {
  const { applicationId } = useParams();

  // Shares the Overview page's query; React Query serves the cached
  // result, so this does not trigger an additional network request.
  const { data: application } = useApplication(applicationId!);
  const [recording, setRecording] = useState(false);

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

          {/*
            Shown whether or not it is known. An application without a number
            is the ordinary state of a new one, and saying so is more useful
            than an empty space that reads like a rendering bug.
          */}
          <Button
            variant="link"
            className="h-auto p-0 text-sm"
            onClick={() => setRecording(true)}
          >
            {application.applicationNumber
              ? `Application Number ${application.applicationNumber}`
              : "Record application number"}
          </Button>
        </div>

        <ApplicationStatusBadge status={application.status} />
      </div>

      <RecordApplicationNumberDialog
        applicationId={application.id}
        currentNumber={application.applicationNumber}
        open={recording}
        onOpenChange={setRecording}
      />
    </header>
  );
}
