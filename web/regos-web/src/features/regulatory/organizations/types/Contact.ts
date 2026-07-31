export interface ContactRole {
  roleId: string;
  code: string;
  name: string;
}

/**
 * A person as both the organization list and the tenant-wide directory return
 * them — one shape, because the server answers both questions with ContactRow.
 */
export interface Contact {
  contactId: string;
  firstName: string;
  lastName: string;
  title: string | null;
  department: string | null;
  organizationId: string;
  organizationName: string;
  siteId: string | null;
  siteName: string | null;
  status: string;
  statusDate: string;
  roles: ContactRole[];
  emails: string[];
  phones: string[];
}
