import { Link, useParams } from "react-router-dom";

import { Page } from "@/shared/components/Page";
import { PageHeader } from "@/shared/components/PageHeader";

import { ResponseDue } from "../components/ResponseDue";
import { directionLabel } from "../constants/correspondenceDirections";
import { useCorrespondence } from "../hooks/useCorrespondence";

/**
 * One letter, and everything currently known about it.
 *
 * There is no status here, and there is not meant to be: a letter that has been
 * received does not change (ADR-040 decision 4). Questions (S003) and
 * attachments (S002) attach to this page as they arrive.
 */
export function CorrespondenceDetailPage() {
  const { correspondenceId } = useParams();

  const { data: letter, isLoading, error } = useCorrespondence(
    correspondenceId!,
  );

  if (isLoading) {
    return (
      <Page>
        <p className="text-muted-foreground">Loading correspondence...</p>
      </Page>
    );
  }

  if (error) {
    return (
      <Page>
        <p className="text-destructive">Failed to load this correspondence.</p>
      </Page>
    );
  }

  if (!letter) {
    return (
      <Page>
        <div
          className="rounded-lg border border-dashed p-8 text-center"
          data-testid="correspondence-not-found"
        >
          <h3 className="text-lg font-semibold">
            This correspondence does not exist.
          </h3>
        </div>
      </Page>
    );
  }

  return (
    <Page>
      <PageHeader
        title={letter.subject}
        description={`${directionLabel(letter.direction)} · ${letter.authorityName}`}
      />

      <div className="rounded-lg border p-6">
        <dl className="grid gap-4 sm:grid-cols-2">
          <div>
            <dt className="text-sm text-muted-foreground">Health authority</dt>
            <dd className="font-medium">{letter.authorityName}</dd>
          </div>

          <div>
            <dt className="text-sm text-muted-foreground">Type</dt>
            <dd className="font-medium">{letter.correspondenceTypeName}</dd>
          </div>

          <div>
            <dt className="text-sm text-muted-foreground">Received or sent</dt>
            <dd className="font-medium">{directionLabel(letter.direction)}</dd>
          </div>

          <div>
            {/* "Dated", not "Date" — "Recorded" sits beside it, and two dates
                sharing a label is a page nobody can read aloud. */}
            <dt className="text-sm text-muted-foreground">Dated</dt>
            <dd className="font-medium">{letter.occurredOn}</dd>
          </div>

          <div>
            <dt className="text-sm text-muted-foreground">Response due</dt>
            <dd className="font-medium">
              <ResponseDue dueOn={letter.responseDueOn} />
            </dd>
          </div>

          <div>
            <dt className="text-sm text-muted-foreground">
              Authority reference
            </dt>
            <dd className="font-medium">
              {letter.authorityReference ?? "—"}
            </dd>
          </div>

          <div>
            <dt className="text-sm text-muted-foreground">Filed against</dt>
            <dd className="font-medium">
              {letter.regulatoryApplicationId ? (
                <Link
                  className="underline-offset-4 hover:underline"
                  to={`/regulatory/products`}
                >
                  {letter.regulatoryApplicationName}
                  {letter.regulatoryApplicationNumber
                    ? ` (${letter.regulatoryApplicationNumber})`
                    : ""}
                </Link>
              ) : (
                "Nothing — general correspondence"
              )}
            </dd>
          </div>

          <div>
            {/* Both dates are shown on purpose: a letter logged today may be
                dated 2019, and a reader who cannot see both will eventually
                mistake one for the other. */}
            <dt className="text-sm text-muted-foreground">Recorded in RegOS</dt>
            <dd className="font-medium">
              {new Date(letter.recordedOnUtc).toISOString().slice(0, 10)}
            </dd>
          </div>
        </dl>
      </div>
    </Page>
  );
}
