export interface ProductDocumentSummary {
  id: string;
  name: string;
  documentTypeName: string;
  status: string;
  currentVersionNumber: number | null;
  createdOnUtc: string;
}
