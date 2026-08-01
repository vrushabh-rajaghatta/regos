import { z } from "zod";

export const raiseQuestionSchema = z.object({
  number: z
    .string()
    .min(1, "A question number is required.")
    .max(20, "A question number cannot exceed 20 characters."),

  text: z
    .string()
    .min(1, "The question text is required.")
    .max(4000, "A question cannot exceed 4000 characters."),

  // Ours, not the authority's. The letter carries the regulatory deadline.
  targetResponseOn: z.string().optional(),
});

export type RaiseQuestionFormValues = z.infer<typeof raiseQuestionSchema>;
