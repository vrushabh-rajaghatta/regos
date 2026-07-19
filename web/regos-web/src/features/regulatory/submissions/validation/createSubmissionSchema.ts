import { z } from "zod";

export const createSubmissionSchema = z.object({
  title: z.string().trim().min(1, "Submission title is required."),

  submissionTypeId: z.string().min(1, "Submission Type is required."),
});

export type CreateSubmissionFormValues = z.infer<
  typeof createSubmissionSchema
>;
