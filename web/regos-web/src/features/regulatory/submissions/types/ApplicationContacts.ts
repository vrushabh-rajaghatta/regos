/**
 * Who currently speaks for an application — **derived, never stored**.
 *
 * There is deliberately no application-level contact model (ADR-048): under the
 * cumulative model the latest published sequence *is* the current regulatory
 * state, so a stored copy could only differ from it by being stale.
 */
export interface ApplicationContacts {
  /** Null when nothing has been published — an absence of a filing. */
  asOfSequenceNumber: number | null;
  contacts: ApplicationContact[];
}

export interface ApplicationContact {
  contactId: string;
  contactName: string;
  contactTitle: string | null;
  organizationName: string;
  roleId: string;
  roleName: string;
}
