export interface CountryDto {
  id: string;
  /** ISO 3166-1 alpha-2 — what a picker shows beside the name. */
  code: string;
  /**
   * ISO 3166-1 alpha-3 — what a machine-readable submission names the country
   * by, and **not derivable** from `code`.
   */
  isoAlpha3Code: string;
  name: string;
  /**
   * The register's own wording — "United Kingdom of Great Britain and Northern
   * Ireland". A screen keeps showing `name`; this is for callers that must
   * quote the official form.
   */
  isoName: string;
  /**
   * The regulatory groupings this country belongs to. They overlap — Germany is
   * EU and ICH and PIC/S — and empty is a recorded answer, not a blank.
   */
  regions: string[];
}
