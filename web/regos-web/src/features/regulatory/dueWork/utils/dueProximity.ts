/**
 * What a due date means today.
 *
 * Derived at the edge, stored nowhere and computed nowhere else: "overdue"
 * changes every midnight, and a value like that persisted in a database is
 * wrong for most of its life (ADR-037).
 */
export type DueProximity = "overdue" | "today" | "soon" | "later" | "undated";

export function dueProximity(dueOn: string | null): DueProximity {
  if (!dueOn) return "undated";

  const today = new Date();
  today.setHours(0, 0, 0, 0);

  const due = new Date(`${dueOn}T00:00:00`);
  const days = Math.round((due.getTime() - today.getTime()) / 86_400_000);

  if (days < 0) return "overdue";
  if (days === 0) return "today";
  if (days <= 14) return "soon";
  return "later";
}

export function dueLabel(dueOn: string | null): string {
  if (!dueOn) return "No date";

  const today = new Date();
  today.setHours(0, 0, 0, 0);

  const due = new Date(`${dueOn}T00:00:00`);
  const days = Math.round((due.getTime() - today.getTime()) / 86_400_000);

  if (days < 0) {
    const late = Math.abs(days);
    return `${dueOn} · overdue by ${late} ${late === 1 ? "day" : "days"}`;
  }

  if (days === 0) return `${dueOn} · today`;

  return `${dueOn} · in ${days} ${days === 1 ? "day" : "days"}`;
}
