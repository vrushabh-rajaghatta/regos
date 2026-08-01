export interface CorrespondenceDetail {
  correspondenceId: string;
  direction: string;
  subject: string;
  occurredOn: string;
  responseDueOn: string | null;
  authorityReference: string | null;
  recordedOnUtc: string;
  authorityId: string;
  authorityName: string;
  correspondenceTypeId: string;
  correspondenceTypeName: string;
  authorityDivisionId: string | null;
  authorityDivisionName: string | null;
  regulatoryApplicationId: string | null;
  regulatoryApplicationName: string | null;
  regulatoryApplicationNumber: string | null;
  submissionId: string | null;
  registrationId: string | null;
}
