import { z } from "zod";

import { SUBMISSION_FORMATS } from "../utils/formatLabel";

export const createSubmissionSchema = z.object({
  title: z.string().trim().min(1, "Submission title is required."),


  // Required rather than defaulted in the schema, for the reason ADR-047
  // gives: the filer chooses the format, and a default lets the form answer
  // for them. The dialog opens with eCTD selected, which is a stated choice
  // the user can see and change — not an omission.
  format: z.enum(SUBMISSION_FORMATS, "Submission Format is required."),
});

export type CreateSubmissionFormValues = z.infer<
  typeof createSubmissionSchema
>;
