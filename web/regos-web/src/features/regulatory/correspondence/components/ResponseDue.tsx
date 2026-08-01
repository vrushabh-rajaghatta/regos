interface ResponseDueProps {
  dueOn: string | null;
}

/**
 * Proximity to a response deadline, derived here and stored nowhere.
 *
 * The server returns the date; "in 9 days" is the reader's interpretation of
 * it, and it changes every midnight. Deriving it at the edge keeps one clock in
 * play instead of three (ADR-037, and EPIC-005's precedent for expiry).
 */
export function ResponseDue({ dueOn }: ResponseDueProps) {
  if (!dueOn) {
    return <span className="text-muted-foreground">—</span>;
  }

  const today = new Date();
  today.setHours(0, 0, 0, 0);

  const due = new Date(`${dueOn}T00:00:00`);
  const days = Math.round((due.getTime() - today.getTime()) / 86_400_000);

  if (days < 0) {
    return (
      <span className="font-medium text-destructive">
        {dueOn} · overdue by {Math.abs(days)} {Math.abs(days) === 1 ? "day" : "days"}
      </span>
    );
  }

  if (days === 0) {
    return <span className="font-medium text-destructive">{dueOn} · today</span>;
  }

  return (
    <span className={days <= 14 ? "font-medium text-amber-600" : undefined}>
      {dueOn} · in {days} {days === 1 ? "day" : "days"}
    </span>
  );
}
