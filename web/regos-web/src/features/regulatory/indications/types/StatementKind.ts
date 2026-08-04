/**
 * Which clinical statement a population qualifies.
 *
 * The three route bases, and nothing more. The population form, its schema and
 * its save call are identical across all three — the frontend analogue of the
 * persistence helper S004 earned, and for the same reason: one shape, three
 * owners.
 */
export type StatementKind =
  | "indications"
  | "contraindications"
  | "undesirable-effects";
