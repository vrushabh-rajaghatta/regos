import { z } from "zod";

import { SUBMISSION_FORMATS } from "../utils/formatLabel";

/**
 * Which regulatory activity a sequence belongs to. The two options are the
 * form's version of the exclusive-or the domain type enforces: a sequence
 * either opens an activity or continues one, never both.
 */
export const ACTIVITY_CHOICES = ["start", "continue"] as const;

export const createSubmissionSchema = z
  .object({
    title: z.string().trim().min(1, "Submission title is required."),

    // Required rather than defaulted in the schema, for the reason ADR-047
    // gives: the filer chooses the format, and a default lets the form answer
    // for them. The dialog opens with eCTD selected, which is a stated choice
    // the user can see and change — not an omission.
    format: z.enum(SUBMISSION_FORMATS, "Submission Format is required."),

    activityChoice: z.enum(ACTIVITY_CHOICES),

    submissionTypeId: z.string().optional(),

    originatingSubmissionId: z.string().optional(),

    // No default, and deliberately unlike format. An omitted format has one
    // honest reading; an omitted sequence action has none — an opening sequence
    // can perfectly well be a Report rather than an Application (evidence E13),
    // so there is no value that "obviously" belongs.
    submissionSubTypeId: z
      .string()
      .min(1, "What this sequence does is required."),
  })
  .superRefine((values, ctx) => {
    // The two branches are checked separately rather than by one "exactly one
    // of these" rule, so the error lands on the control the user is looking at
    // rather than on the form as a whole.
    if (values.activityChoice === "start" && !values.submissionTypeId) {
      ctx.addIssue({
        code: "custom",
        path: ["submissionTypeId"],
        message: "Choose what regulatory activity this starts.",
      });
    }

    if (
      values.activityChoice === "continue" &&
      !values.originatingSubmissionId
    ) {
      ctx.addIssue({
        code: "custom",
        path: ["originatingSubmissionId"],
        message: "Choose the regulatory activity this continues.",
      });
    }
  });

export type CreateSubmissionFormValues = z.infer<typeof createSubmissionSchema>;
