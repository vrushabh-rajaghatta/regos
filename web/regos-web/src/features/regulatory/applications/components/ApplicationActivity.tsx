export interface ApplicationActivityItem {
  type: string;
  occurredOn: string;
}

// Business events, not raw timestamps: the type maps to a domain-language label.
const ACTIVITY_LABELS: Record<string, string> = {
  Created: "Application created",
};

interface ApplicationActivityProps {
  activities: ApplicationActivityItem[];
}

export function ApplicationActivity({ activities }: ApplicationActivityProps) {
  return (
    <ul className="space-y-3">
      {activities.map((activity) => (
        <li
          key={`${activity.type}-${activity.occurredOn}`}
          className="flex items-start gap-3"
        >
          <span className="mt-1.5 size-2 shrink-0 rounded-full bg-muted-foreground/50" />

          <div>
            <div className="text-sm font-medium">
              {ACTIVITY_LABELS[activity.type] ?? activity.type}
            </div>

            <div className="text-xs text-muted-foreground">
              {new Date(activity.occurredOn).toLocaleDateString(undefined, {
                day: "2-digit",
                month: "short",
                year: "numeric",
              })}
            </div>
          </div>
        </li>
      ))}
    </ul>
  );
}
