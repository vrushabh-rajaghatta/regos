/**
 * The languages the trade-name picker offers.
 *
 * **Presentation vocabulary, not domain data** (SC-105). The domain stores a
 * validated ISO 639-1 code and no rule branches on it, so there is no
 * `Language` reference-data aggregate to read this from — governed reference
 * data exists because the domain needs governed facts, not because dropdowns
 * need labels.
 *
 * The readable names come from `Intl.DisplayNames`, so they arrive already
 * translated into the reader's own locale and there is no name list to
 * maintain. The codes are curated because two hundred of them in a dropdown is
 * not a shorter journey than typing two letters.
 */
const LANGUAGE_CODES = [
  "en",
  "fr",
  "de",
  "es",
  "it",
  "pt",
  "nl",
  "sv",
  "da",
  "fi",
  "pl",
  "cs",
  "el",
  "ja",
  "zh",
  "ko",
  "ar",
  "he",
  "ru",
  "tr",
  "hi",
] as const;

export interface LanguageOption {
  code: string;
  name: string;
}

/**
 * Falls back to the bare code where the runtime has no display name for it —
 * an unlabelled option a user can still choose beats a missing one.
 */
export function languageName(code: string): string {
  try {
    return new Intl.DisplayNames(undefined, { type: "language" }).of(code)
      ?? code;
  } catch {
    return code;
  }
}

export const LANGUAGES: LanguageOption[] = LANGUAGE_CODES
  .map((code) => ({ code, name: languageName(code) }))
  .sort((a, b) => a.name.localeCompare(b.name));
