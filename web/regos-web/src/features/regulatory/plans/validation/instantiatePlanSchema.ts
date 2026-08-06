import { z } from "zod";

/**
 * Shape and required-ness only. The server owns the rules — that the version is
 * published, that the objective exists — and renders its refusal verbatim.
 *
 * The anchor date is required rather than defaulted to today, deliberately: a
 * plan's schedule is derived from it once and never recalculated, so it is a
 * decision rather than a convenience.
 */
export const instantiatePlanSchema = z.object({
  processDefinitionId: z.string().min(1, "Choose a playbook."),

  processDefinitionVersionId: z.string().min(1, "Choose a published version."),

  anchorDate: z.string().min(1, "Choose the date the plan starts from."),

  name: z
    .string()
    .trim()
    .min(1, "Give the plan a name.")
    .max(300, "That name is 300 characters at most."),
});

export type InstantiatePlanValues = z.infer<typeof instantiatePlanSchema>;
