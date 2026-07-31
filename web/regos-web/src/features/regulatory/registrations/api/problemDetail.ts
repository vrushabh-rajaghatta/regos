/**
 * Reads the server's own words out of a ProblemDetails response.
 *
 * The lifecycle and policy messages are written for a regulatory reader
 * ("A Withdrawn registration has reached the end of its lifecycle"), so they
 * are surfaced verbatim rather than paraphrased into a second copy of the
 * domain's vocabulary.
 */
export async function detailOf(
  response: Response,
  fallback: string
): Promise<string> {
  try {
    const problem = await response.json();
    return typeof problem?.detail === "string" ? problem.detail : fallback;
  } catch {
    return fallback;
  }
}
