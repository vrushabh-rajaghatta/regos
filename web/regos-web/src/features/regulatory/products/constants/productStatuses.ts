// Mirrors ProductStatus in the domain. Two states is the entire lifecycle:
// Registered -> Archived. See the enum for why there is no "Active".
export const PRODUCT_STATUSES = [
  { value: "Registered", label: "Registered" },
  { value: "Archived", label: "Archived" },
] as const;
