export interface Correspondence {
  correspondenceId: string;
  direction: string;
  subject: string;
  occurredOn: string;
  responseDueOn: string | null;
  authorityReference: string | null;
  authorityId: string;
  authorityName: string;
  correspondenceTypeId: string;
  correspondenceTypeName: string;
  regulatoryApplicationId: string | null;
  regulatoryApplicationNumber: string | null;
}
