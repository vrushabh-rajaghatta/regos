/**
 * Mirrors OrganizationSiteType on the server, which is a closed enum rather
 * than reference data because rules branch on it — only a Manufacturing site
 * can be named on a licence as an approved manufacturer.
 *
 * Kept as an explicit list rather than derived from returned rows: the create
 * form has to offer every type, including ones this tenant has never used.
 */
export const ORGANIZATION_SITE_TYPES = [
  { value: "Manufacturing", label: "Manufacturing" },
  { value: "Packaging", label: "Packaging" },
  { value: "Testing", label: "Testing" },
  { value: "Storage", label: "Storage" },
  { value: "AuthorityOffice", label: "Authority Office" },
  { value: "Office", label: "Office" },
] as const;

export function siteTypeLabel(value: string): string {
  return (
    ORGANIZATION_SITE_TYPES.find((type) => type.value === value)?.label ?? value
  );
}
