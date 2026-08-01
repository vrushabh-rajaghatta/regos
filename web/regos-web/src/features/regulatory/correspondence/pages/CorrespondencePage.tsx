import { useState } from "react";
import { Link } from "react-router-dom";

import { Button } from "@/components/ui/button";
import { Page } from "@/shared/components/Page";
import { PageHeader } from "@/shared/components/PageHeader";

import { useAuthorities } from "../../masterData/hooks/useAuthorities";
import { RecordCorrespondenceDialog } from "../components/RecordCorrespondenceDialog";
import { ResponseDue } from "../components/ResponseDue";
import { CORRESPONDENCE_DIRECTIONS, directionLabel } from "../constants/correspondenceDirections";
import { useCorrespondenceList } from "../hooks/useCorrespondenceList";
import { useCorrespondenceTypes } from "../hooks/useCorrespondenceTypes";

/**
 * Everything that has passed between us and an authority, newest first.
 *
 * A flat list rather than a per-application section, because that is how the
 * question is actually asked — *"what came in this week?"* precedes knowing
 * which application it was about. Filtering narrows it to the other three
 * questions: by authority, by kind, by direction.
 */
export function CorrespondencePage() {
  const [logging, setLogging] = useState(false);
  const [authorityId, setAuthorityId] = useState("");
  const [correspondenceTypeId, setCorrespondenceTypeId] = useState("");
  const [direction, setDirection] = useState("");

  const authorities = useAuthorities();
  const types = useCorrespondenceTypes();

  const { data, isLoading, error } = useCorrespondenceList({
    authorityId: authorityId || undefined,
    correspondenceTypeId: correspondenceTypeId || undefined,
    direction: direction || undefined,
  });

  const rows = data ?? [];

  return (
    <Page>
      <PageHeader
        title="Correspondence"
        description="Letters, emails and formal communications with a health authority."
        actions={
          <Button onClick={() => setLogging(true)}>Log correspondence</Button>
        }
      />

      <div className="mb-4 flex flex-wrap gap-3">
        <select
          aria-label="Filter by authority"
          className="h-9 rounded-md border bg-transparent px-3 text-sm"
          value={authorityId}
          onChange={(event) => setAuthorityId(event.target.value)}
        >
          <option value="">All authorities</option>
          {(authorities.data ?? []).map((authority) => (
            <option key={authority.id} value={authority.id}>
              {authority.name}
            </option>
          ))}
        </select>

        <select
          aria-label="Filter by type"
          className="h-9 rounded-md border bg-transparent px-3 text-sm"
          value={correspondenceTypeId}
          onChange={(event) => setCorrespondenceTypeId(event.target.value)}
        >
          <option value="">All types</option>
          {(types.data ?? []).map((type) => (
            <option key={type.id} value={type.id}>
              {type.name}
            </option>
          ))}
        </select>

        <select
          aria-label="Filter by direction"
          className="h-9 rounded-md border bg-transparent px-3 text-sm"
          value={direction}
          onChange={(event) => setDirection(event.target.value)}
        >
          <option value="">Received and sent</option>
          {CORRESPONDENCE_DIRECTIONS.map((option) => (
            <option key={option.value} value={option.value}>
              {option.label}
            </option>
          ))}
        </select>
      </div>

      {isLoading && <p className="text-muted-foreground">Loading correspondence...</p>}

      {error && (
        <p className="text-destructive">Failed to load correspondence.</p>
      )}

      {!isLoading && !error && rows.length === 0 && (
        <div
          className="rounded-lg border border-dashed p-8 text-center"
          data-testid="correspondence-empty"
        >
          <h3 className="text-lg font-semibold">No correspondence yet.</h3>
          <p className="mt-1 text-sm text-muted-foreground">
            Log the first letter to start the record.
          </p>
        </div>
      )}

      {rows.length > 0 && (
        <div className="overflow-x-auto rounded-lg border">
          <table className="w-full text-sm">
            <thead className="bg-muted/50 text-left">
              <tr>
                <th className="px-4 py-2 font-medium">Subject</th>
                <th className="px-4 py-2 font-medium">Authority</th>
                <th className="px-4 py-2 font-medium">Type</th>
                <th className="px-4 py-2 font-medium">Direction</th>
                <th className="px-4 py-2 font-medium">Dated</th>
                <th className="px-4 py-2 font-medium">Response due</th>
              </tr>
            </thead>

            <tbody>
              {rows.map((row) => (
                <tr key={row.correspondenceId} className="border-t">
                  <td className="px-4 py-2">
                    <Link
                      className="font-medium underline-offset-4 hover:underline"
                      to={`/regulatory/correspondence/${row.correspondenceId}`}
                    >
                      {row.subject}
                    </Link>
                  </td>
                  <td className="px-4 py-2">{row.authorityName}</td>
                  <td className="px-4 py-2">{row.correspondenceTypeName}</td>
                  <td className="px-4 py-2">{directionLabel(row.direction)}</td>
                  <td className="px-4 py-2">{row.occurredOn}</td>
                  <td className="px-4 py-2">
                    <ResponseDue dueOn={row.responseDueOn} />
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      <RecordCorrespondenceDialog open={logging} onOpenChange={setLogging} />
    </Page>
  );
}
