import { useState } from "react";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Page } from "@/shared/components/Page";
import { PageHeader } from "@/shared/components/PageHeader";

import { BeginInspectionDialog } from "../components/BeginInspectionDialog";
import { useChangeInspectionStatus } from "../hooks/useChangeInspectionStatus";
import { useInspections } from "../hooks/useInspections";
import type { Inspection } from "../api/listInspections";

/**
 * Authority inspections.
 *
 * Like a meeting, an inspection concludes: it leaves this list once completed
 * or cancelled, because the list answers "what is coming?" What remains after
 * it are commitments — the corrective actions its findings oblige — which live
 * on their own aggregate with their own due dates.
 */
export function InspectionsPage() {
  const [recording, setRecording] = useState(false);
  const [includeConcluded, setIncludeConcluded] = useState(false);

  const { data, isLoading, error } = useInspections(includeConcluded);
  const rows = data ?? [];

  return (
    <Page>
      <PageHeader
        title="Inspections"
        description="What an authority came to see, and what they found."
        actions={
          <Button onClick={() => setRecording(true)}>Record inspection</Button>
        }
      />

      <div className="mb-4">
        <Button
          type="button"
          variant={includeConcluded ? "default" : "outline"}
          onClick={() => setIncludeConcluded(!includeConcluded)}
        >
          {includeConcluded ? "Showing all" : "Show concluded too"}
        </Button>
      </div>

      {isLoading && <p className="text-muted-foreground">Loading inspections...</p>}
      {error && <p className="text-destructive">Failed to load inspections.</p>}

      {!isLoading && !error && rows.length === 0 && (
        <div
          className="rounded-lg border border-dashed p-8 text-center"
          data-testid="inspections-empty"
        >
          <h3 className="text-lg font-semibold">No inspections yet.</h3>
        </div>
      )}

      <ul className="space-y-4">
        {rows.map((inspection) => (
          <InspectionCard key={inspection.inspectionId} inspection={inspection} />
        ))}
      </ul>

      <BeginInspectionDialog open={recording} onOpenChange={setRecording} />
    </Page>
  );
}

function InspectionCard({ inspection }: { inspection: Inspection }) {
  const [occurredOn, setOccurredOn] = useState("");
  const change = useChangeInspectionStatus(inspection.inspectionId);

  const next =
    inspection.currentStatus === "Announced"
      ? ["InProgress", "Completed", "Cancelled"]
      : inspection.currentStatus === "InProgress"
        ? ["Completed", "Cancelled"]
        : [];

  return (
    <li className="rounded-lg border p-6" data-testid="inspection-card">
      <h3 className="text-lg font-semibold">{inspection.title}</h3>

      <p className="mt-1 text-sm text-muted-foreground">
        {inspection.authorityName}
        {" · "}
        {inspection.currentStatus}
        {inspection.scheduledFor ? ` · scheduled ${inspection.scheduledFor}` : ""}
        {inspection.completedOn ? ` · completed ${inspection.completedOn}` : ""}
      </p>

      <p className="mt-1 text-sm">
        {/* The site is what was inspected, not metadata about the record. */}
        <span className="text-muted-foreground">Site inspected: </span>
        {inspection.organizationSiteName ?? "Not yet known"}
      </p>

      {inspection.outcome && (
        <div className="mt-3 rounded-md bg-muted/40 p-3 text-sm">
          <p className="font-medium">What the authority found</p>
          <p className="mt-1">{inspection.outcome}</p>
        </div>
      )}

      <ol className="mt-3 space-y-1 border-l pl-4" data-testid="inspection-history">
        {inspection.history.map((entry, index) => (
          <li key={`${entry.status}-${index}`} className="text-sm">
            <span className="font-medium">{entry.status}</span>
            <span className="text-muted-foreground"> · {entry.occurredOn}</span>
          </li>
        ))}
      </ol>

      {next.length > 0 && (
        <div className="mt-3 flex flex-wrap items-end gap-2">
          <label className="text-sm" htmlFor={`on-${inspection.inspectionId}`}>
            On
            <Input
              id={`on-${inspection.inspectionId}`}
              type="date"
              className="mt-1"
              value={occurredOn}
              onChange={(event) => setOccurredOn(event.target.value)}
            />
          </label>

          {next.map((status) => (
            <Button
              key={status}
              type="button"
              variant="outline"
              disabled={change.isPending || !occurredOn}
              onClick={() =>
                change.mutateAsync({ status, occurredOn }).catch(() => undefined)
              }
            >
              {status}
            </Button>
          ))}
        </div>
      )}

      {change.isError && (
        <p className="mt-2 text-sm text-destructive" data-testid="inspection-error">
          {(change.error as Error).message}
        </p>
      )}
    </li>
  );
}
