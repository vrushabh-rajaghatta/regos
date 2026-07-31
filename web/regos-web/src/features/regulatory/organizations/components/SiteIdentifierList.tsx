import type { SiteIdentifier } from "../types/SiteIdentifier";

/**
 * A site's registry identifiers, inline. Read-only: a site has no
 * add-identifier command of its own yet — identifiers are recorded when the
 * site is created.
 */
export function SiteIdentifierList({
  identifiers,
}: {
  identifiers: SiteIdentifier[];
}) {
  if (identifiers.length === 0) return null;

  return (
    <p className="text-sm text-muted-foreground">
      {identifiers.map((identifier, index) => (
        <span key={identifier.id}>
          {index > 0 && " · "}
          {identifier.schemeCode}{" "}
          <span className="font-mono">{identifier.value}</span>
        </span>
      ))}
    </p>
  );
}
