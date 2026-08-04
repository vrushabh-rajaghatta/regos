import { z } from "zod";

/**
 * The pack-size pair is refused in halves, matching the aggregate: *30* alone
 * could be tablets, millilitres or vials, and a unit with no quantity says
 * nothing at all. Both empty is valid — a pack whose size is not settled yet.
 *
 * **A factory rather than one schema**, because adding a pack and correcting one
 * take different fields: `statusDate` starts the commercial history and is asked
 * for once, while restating what a pack *is* must not move that history.
 *
 * A single schema requiring it made the correct-form fail validation against a
 * field it never rendered — submitting did nothing, with no error anywhere,
 * which is worse than the visible refusal SC-106 exists to guarantee.
 */
export function packSchema(requireStatusDate: boolean) {
  return z
    .object({
      description: z
        .string()
        .trim()
        .min(1, "Describe the pack — what a person reads off the carton.")
        .max(250),

      packSizeQuantity: z.string().trim(),
      packSizeUnitCode: z.string().trim(),

      packCode: z.string().trim().max(50).optional(),

      // The business date the pack came into being. Never defaulted to today,
      // so a portfolio carried over from a legacy system can say when it was.
      statusDate: z.string(),
    })
    .refine((v) => !(v.packSizeQuantity !== "" && v.packSizeUnitCode === ""), {
      path: ["packSizeUnitCode"],
      message: "A pack size needs a unit.",
    })
    .refine((v) => !(v.packSizeUnitCode !== "" && v.packSizeQuantity === ""), {
      path: ["packSizeQuantity"],
      message: "A pack size unit needs a quantity.",
    })
    .refine((v) => !requireStatusDate || v.statusDate !== "", {
      path: ["statusDate"],
      message: "A date is required.",
    });
}

export type PackFormValues = z.infer<ReturnType<typeof packSchema>>;

export const packMarketingStatusSchema = z.object({
  status: z.enum(["Marketed", "TemporarilyUnavailable", "Discontinued"]),

  occurredOn: z.string().min(1, "A date is required."),

  note: z.string().trim().max(500).optional(),
});

export type PackMarketingStatusFormValues = z.infer<
  typeof packMarketingStatusSchema
>;
