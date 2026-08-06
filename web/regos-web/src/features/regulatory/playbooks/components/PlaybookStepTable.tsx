import type { PlaybookStep } from "../types/PlaybookDetail";

/**
 * The authored steps of one version.
 *
 * Deliberately shows **no dates**. A definition describes work; dates exist only
 * once a plan is instantiated from it, and showing a date here would suggest the
 * playbook has a schedule of its own. What it shows instead is the two numbers a
 * plan is derived from — when the step starts relative to what it waits for, and
 * how long it runs.
 */
export function PlaybookStepTable({ steps }: { steps: PlaybookStep[] }) {
  return (
    <div className="overflow-x-auto rounded-lg border">
      <table className="w-full text-sm" data-testid="playbook-steps">
        <thead className="bg-muted/50 text-left">
          <tr>
            <th className="p-3 font-medium">Step</th>
            <th className="p-3 font-medium">Waits for</th>
            <th className="p-3 font-medium whitespace-nowrap">Starts</th>
            <th className="p-3 font-medium whitespace-nowrap">Takes</th>
          </tr>
        </thead>

        <tbody>
          {steps.map((step) => (
            <tr
              key={step.id}
              data-testid="playbook-step-row"
              className="border-t align-top"
            >
              <td className="p-3">
                <div className="font-medium">{step.name}</div>

                <div className="mt-0.5 font-mono text-xs text-muted-foreground">
                  {step.code}
                </div>

                {step.description && (
                  <p className="mt-1 max-w-prose text-xs text-muted-foreground">
                    {step.description}
                  </p>
                )}
              </td>

              <td className="p-3">
                {step.predecessors.length === 0 ? (
                  <span className="text-muted-foreground">
                    the plan&rsquo;s start
                  </span>
                ) : (
                  <div className="flex flex-wrap gap-1">
                    {step.predecessors.map((code) => (
                      <span
                        key={code}
                        className="rounded bg-muted px-1.5 py-0.5 font-mono text-xs"
                      >
                        {code}
                      </span>
                    ))}
                  </div>
                )}
              </td>

              <td className="p-3 whitespace-nowrap text-muted-foreground">
                {step.offsetDays === 0
                  ? "immediately after"
                  : `+${step.offsetDays} days`}
              </td>

              <td className="p-3 whitespace-nowrap text-muted-foreground">
                {step.durationDays} {step.durationDays === 1 ? "day" : "days"}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
