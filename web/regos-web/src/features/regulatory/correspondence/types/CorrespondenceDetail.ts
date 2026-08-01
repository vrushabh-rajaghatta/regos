export interface CorrespondenceAttachmentSummary {
  attachmentId: string;
  originalFileName: string;
  contentType: string;
  fileSizeBytes: number;
  uploadedOnUtc: string;
}

export interface QuestionHistoryEntry {
  status: string;
  occurredOn: string;
  recordedOnUtc: string;
  note: string | null;
}

export interface CorrespondenceQuestionSummary {
  questionId: string;
  number: string;
  text: string;
  targetResponseOn: string | null;
  responseText: string | null;
  currentStatus: string;
  respondedOn: string | null;
  history: QuestionHistoryEntry[];
}

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
  attachments: CorrespondenceAttachmentSummary[];
  questions: CorrespondenceQuestionSummary[];
}
