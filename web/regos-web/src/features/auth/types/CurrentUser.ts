export type UserRole =
  | "PlatformAdministrator"
  | "TenantAdministrator"
  | "Member";

export interface CurrentUser {
  userId: string;
  /** Null for a platform user, whose token carries no tenant claim. */
  tenantId: string | null;
  email: string;
  role: UserRole;
}
