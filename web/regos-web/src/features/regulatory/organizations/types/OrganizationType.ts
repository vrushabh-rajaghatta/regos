/**
 * Mirrors OrganizationType in the domain. The API serializes enums as strings
 * (JsonStringEnumConverter), so these values travel as-is and need no mapping.
 */
export const ORGANIZATION_TYPES = [
  { value: "Manufacturer", label: "Manufacturer" },
  { value: "Sponsor", label: "Sponsor" },
  {
    value: "MarketingAuthorizationHolder",
    label: "Marketing Authorization Holder",
  },
  {
    value: "ContractResearchOrganization",
    label: "Contract Research Organization",
  },
] as const;

export type OrganizationTypeValue =
  (typeof ORGANIZATION_TYPES)[number]["value"];

export function organizationTypeLabel(value: string): string {
  return ORGANIZATION_TYPES.find((type) => type.value === value)?.label ?? value;
}
