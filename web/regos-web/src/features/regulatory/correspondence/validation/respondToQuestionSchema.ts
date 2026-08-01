import { z } from "zod";

export const respondToQuestionSchema = z.object({
  responseText: z
    .string()
    .min(1, "A response is required.")
    .max(8000, "A response cannot exceed 8000 characters."),

  occurredOn: z.string().min(1, "A date is required."),
});

export type RespondToQuestionFormValues = z.infer<
  typeof respondToQuestionSchema
>;
