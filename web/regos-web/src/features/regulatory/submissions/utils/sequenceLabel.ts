/**
 * How a submission is named on screen.
 *
 * The domain calls it a Submission; a regulatory user says "sequence 0003"
 * (ADR-044 decision 3). Both words are binding, and this is the one place the
 * screen's word is formed — four digits, zero padded, the way eCTD writes it.
 */
export function sequenceLabel(sequenceNumber: number): string {
  return `Sequence ${sequenceNumber.toString().padStart(4, "0")}`;
}

/**
 * What a draft is allowed to say about a number it does not have.
 *
 * A draft labelled "0004" asserts a fact it has not earned — the number is
 * claimed at publish, and whichever draft publishes first takes it. So the
 * wording is an expectation, not an identity: two drafts in one application
 * both read the same next number, and that is true rather than a bug.
 */
export function nextSequenceLabel(nextSequenceNumber: number): string {
  return `Will publish as next sequence (currently ${nextSequenceNumber
    .toString()
    .padStart(4, "0")})`;
}
