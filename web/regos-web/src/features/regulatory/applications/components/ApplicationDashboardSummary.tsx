export interface ApplicationDashboardSummaryItem {
  title: string;
  value: number;
}

interface ApplicationDashboardSummaryProps {
  items: ApplicationDashboardSummaryItem[];
}

export function ApplicationDashboardSummary({
  items,
}: ApplicationDashboardSummaryProps) {
  return (
    <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
      {items.map((item) => (
        <div key={item.title} className="rounded-lg border p-4">
          <div className="text-sm text-muted-foreground">{item.title}</div>
          <div className="mt-1 text-2xl font-semibold">{item.value}</div>
        </div>
      ))}
    </div>
  );
}
