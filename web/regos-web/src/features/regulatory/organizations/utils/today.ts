/**
 * Today as the ISO date the API expects. Forms default to it because the
 * common case is recording something as it happens; an importer states the
 * real date instead.
 */
export function today(): string {
  return new Date().toISOString().slice(0, 10);
}
