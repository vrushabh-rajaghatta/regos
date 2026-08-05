import { z } from "zod";

import { NO_SPECIAL_PRECAUTIONS } from "../types/Supply";

/**
 * The same rules the aggregate enforces, so a refusal is visible before the
 * round trip rather than only after it.
 *
 * **Every rule here has a server-side twin**, deliberately. The schema exists to
 * answer sooner, never to be the only answer — a browser is not where a
 * regulatory invariant lives.
 */
export const packSupplySchema = z
  .object({
    legalStatusOfSupplyCode: z.string().trim(),

    shelfLifeValue: z.string().trim(),

    shelfLifeUnitCode: z.string().trim(),

    shelfLifeText: z.string().trim().max(1000).optional(),

    storageConditionCodes: z.array(z.string()),

    // No rule of its own, and none is missing. A pack may say where its data
    // came from before anyone has decided how long it keeps, and a market that
    // does not accept the condition is told so on the market view rather than
    // refused here (EPIC-022 D6 — advisory, never blocking).
    testedAtCodes: z.array(z.string()),
  })
  // Half a shelf life is refused for the reason half a pack size is: 36 alone
  // could be days, months or years.
  .refine((v) => !(v.shelfLifeValue !== "" && v.shelfLifeUnitCode === ""), {
    message: "A shelf life needs a period — 36 could be days, months or years.",
    path: ["shelfLifeUnitCode"],
  })
  .refine((v) => !(v.shelfLifeUnitCode !== "" && v.shelfLifeValue === ""), {
    message: "A shelf-life period needs a number.",
    path: ["shelfLifeValue"],
  })
  .refine((v) => v.shelfLifeValue === "" || Number(v.shelfLifeValue) > 0, {
    message: "A shelf life must be greater than zero.",
    path: ["shelfLifeValue"],
  })
  // "None needed" is a conclusion about the whole set, so it cannot sit beside
  // a precaution.
  .refine(
    (v) =>
      !(
        v.storageConditionCodes.includes(NO_SPECIAL_PRECAUTIONS) &&
        v.storageConditionCodes.length > 1
      ),
    {
      message:
        '"No special storage precautions" cannot sit beside a precaution — ' +
        "remove one or the other.",
      path: ["storageConditionCodes"],
    },
  );

export type PackSupplyFormValues = z.infer<typeof packSupplySchema>;
