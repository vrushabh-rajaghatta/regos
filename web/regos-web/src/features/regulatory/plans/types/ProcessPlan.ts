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
