/**
 * A term drawn from a controlled vocabulary.
 *
 * `system` travels to the client on purpose. Every term RegOS ships today is
 * `regos-internal`, and a screen showing "Tablet" or "Chemical" without saying
 * whose word it is implies terminology the platform does not hold (ADR-058 §6).
 */
export interface CodedConcept {
  system: string;
  code: string;
  display: string;
}

/**
 * RegOS's own naming authority. Compare against this to say so on screen —
 * never to decide behaviour, which is what the code is for.
 */
export const REGOS_INTERNAL = "regos-internal";
