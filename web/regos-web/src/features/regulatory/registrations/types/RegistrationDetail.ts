export interface RegistrationStatusEntry {
  id: string;
  status: string;
  /** When it happened in the world. */
  occurredOn: string;
  /** When RegOS learned of it. */
  recordedOnUtc: string;
  note: string | null;
}

export interface RegistrationDetail {
  id: string;
  productId: string;
  productName: string;
  countryId: string;
  countryName: string;
  authorityId: string;
  authorityName: string;
  holderOrganizationId: string;
  holderOrganizationName: string;
  originatingApplicationId: string | null;
  registrationNumber: string | null;
  status: string;
  approvedOn: string | null;
  expiresOn: string | null;
  createdOn: string;
  history: RegistrationStatusEntry[];

  /**
   * Where this registration may go from here, decided by the server. The UI
   * never asks "may I show Suspend?" — only "did the server include it?" — so
   * the lifecycle is never restated here. Empty when the status is terminal.
   */
  allowedNextStatuses: string[];
}
