/**
 * Reads the server's own words out of a ProblemDetails response.
 *
 * The domain messages are written for a regulatory reader ("A second VAT number
 * would mean one of them is wrong"), so they are surfaced verbatim rather than
 * paraphrased into a second copy of the domain's vocabulary.
 *
 * Duplicates the registrations slice's copy rather than sharing one. Within
 * this slice it replaces six inline try/catch blocks, which is the extraction
 * ADR-018 asks for; across slices it is the second occurrence, and a shared
 * `src/shared/api` helper waits for the third.
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
