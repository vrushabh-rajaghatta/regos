import { useState } from "react";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Page } from "@/shared/components/Page";
import { PageHeader } from "@/shared/components/PageHeader";

import { BeginMeetingDialog } from "../components/BeginMeetingDialog";
import { useChangeMeetingStatus } from "../hooks/useChangeMeetingStatus";
import { useMeetings } from "../hooks/useMeetings";
import type { Meeting } from "../api/listMeetings";

/**
 * Meetings with an authority.
 *
 * The only object in this context that concludes: its value is the work it
 * produces — commitments, follow-up questions, a recorded position — not a
 * continuing lifecycle of its own. That is why the aggregate behind this page
 * is so small.
 */
export function MeetingsPage() {
  const [recording, setRecording] = useState(false);
  const [includeConcluded, setIncludeConcluded] = useState(false);

  const { data, isLoading, error } = useMeetings(includeConcluded);
  const rows = data ?? [];

  return (
    <Page>
      <PageHeader
        title="Meetings"
        description="Requested, granted or declined, held — and what the authority concluded."
        actions={
          <Button onClick={() => setRecording(true)}>Record meeting</Button>
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

      {isLoading && <p className="text-muted-foreground">Loading meetings...</p>}
      {error && <p className="text-destructive">Failed to load meetings.</p>}

      {!isLoading && !error && rows.length === 0 && (
        <div
          className="rounded-lg border border-dashed p-8 text-center"
          data-testid="meetings-empty"
        >
          <h3 className="text-lg font-semibold">No meetings yet.</h3>
        </div>
      )}

      <ul className="space-y-4">
        {rows.map((meeting) => (
          <MeetingCard key={meeting.meetingId} meeting={meeting} />
        ))}
      </ul>

      <BeginMeetingDialog open={recording} onOpenChange={setRecording} />
    </Page>
  );
}

function MeetingCard({ meeting }: { meeting: Meeting }) {
  const [occurredOn, setOccurredOn] = useState("");
  const change = useChangeMeetingStatus(meeting.meetingId);

  // Which moves the authority's decision allows from here. The UI offers only
  // what the domain permits; the domain refuses the rest regardless.
  const next =
    meeting.currentStatus === "Requested"
      ? ["Granted", "Declined", "Cancelled"]
      : meeting.currentStatus === "Granted"
        ? ["Held", "Cancelled"]
        : [];

  return (
    <li className="rounded-lg border p-6" data-testid="meeting-card">
      <div className="flex items-start justify-between gap-4">
        <div>
          <h3 className="text-lg font-semibold">{meeting.subject}</h3>
          <p className="mt-1 text-sm text-muted-foreground">
            {meeting.authorityName}
            {meeting.authorityDivisionName
              ? ` · ${meeting.authorityDivisionName}`
              : ""}
            {" · "}
            {meeting.currentStatus}
            {meeting.scheduledFor ? ` · scheduled ${meeting.scheduledFor}` : ""}
            {meeting.heldOn ? ` · held ${meeting.heldOn}` : ""}
          </p>
        </div>
      </div>

      {meeting.outcome && (
        <div className="mt-3 rounded-md bg-muted/40 p-3 text-sm">
          <p className="font-medium">What the authority concluded</p>
          <p className="mt-1">{meeting.outcome}</p>
        </div>
      )}

      <ol className="mt-3 space-y-1 border-l pl-4" data-testid="meeting-history">
        {meeting.history.map((entry, index) => (
          <li key={`${entry.status}-${index}`} className="text-sm">
            <span className="font-medium">{entry.status}</span>
            <span className="text-muted-foreground"> · {entry.occurredOn}</span>
          </li>
        ))}
      </ol>

      {next.length > 0 && (
        <div className="mt-3 flex flex-wrap items-end gap-2">
          <label className="text-sm" htmlFor={`on-${meeting.meetingId}`}>
            On
            <Input
              id={`on-${meeting.meetingId}`}
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
        <p className="mt-2 text-sm text-destructive" data-testid="meeting-error">
          {(change.error as Error).message}
        </p>
      )}
    </li>
  );
}
