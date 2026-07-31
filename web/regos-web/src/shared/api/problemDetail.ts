/**
 * Reads the server's own words out of a ProblemDetails response.
 *
 * The domain messages are written for a regulatory reader ("A Withdrawn
 * registration has reached the end of its lifecycle", "A second VAT number
 * would mean one of them is wrong"), so they are surfaced verbatim rather than
 * paraphrased into a second copy of the domain's vocabulary.
 *
 * Shared as of EPIC-017 S001, on the trigger the second copy named for itself:
 * the registrations slice wrote it, the organizations slice duplicated it and
 * recorded that "a shared `src/shared/api` helper waits for the third", and
 * medicinal products is the third. Three demonstrated consumers, one helper —
 * ADR-018 as intended, rather than a symmetry argument.
 */
export async function detailOf(
  response: Response,
  fallback: string,
): Promise<string> {
  try {
    const problem = await response.json();

    return typeof problem?.detail === "string" ? problem.detail : fallback;
  } catch {
    return fallback;
  }
}
