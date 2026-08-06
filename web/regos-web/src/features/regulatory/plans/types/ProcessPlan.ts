export interface ObjectivePlanSummary {
  id: string;
  name: string;
  status: string;
  definitionName: string;
  definitionVersionNumber: number;
  /** A newer playbook version exists. The plan is unaffected. */
  definitionVersionIsSuperseded: boolean;
  anchorDate: string;
  plannedStartOn: string | null;
  plannedEndOn: string | null;
  stepCount: number;
}

export interface StepHistoryEntry {
  status: string;
  occurredOn: string;
  recordedOnUtc: string;
  note: string | null;
}

/** Both dates are inclusive: five days from the 1st ends on the 5th. */
export interface PlannedStep {
  id: string;
  code: string;
  name: string;
  description: string | null;
  parentStepId: string | null;
  order: number;
  plannedStartOn: string;
  plannedEndOn: string;
  /** What this waits for, by step code. */
  predecessors: string[];
  status: string;
  /** Derived from history. Null after a step completed without a recorded start. */
  actualStartOn: string | null;
  actualEndOn: string | null;
  isSettled: boolean;
  /** Append-only. A correction is a new entry, never a rewrite. */
  history: StepHistoryEntry[];
}

/** One row of the plan board — what a team can work on. */
export interface NextStep {
  planId: string;
  planName: string;
  stepId: string;
  code: string;
  name: string;
  status: string;
  plannedStartOn: string;
  plannedEndOn: string;
  /** Every predecessor is settled. Says ready — never done. */
  isReady: boolean;
  waitingOn: string[];
  daysLate: number | null;
  objectiveName: string;
  countryCode: string;
}

export interface PlanDetail {
  id: string;
  name: string;
  status: string;
  processObjectiveId: string;
  objectiveName: string;
  processDefinitionVersionId: string;
  definitionName: string;
  definitionVersionNumber: number;
  definitionVersionIsSuperseded: boolean;
  anchorDate: string;
  openedOn: string;
  plannedStartOn: string | null;
  plannedEndOn: string | null;
  steps: PlannedStep[];
}
