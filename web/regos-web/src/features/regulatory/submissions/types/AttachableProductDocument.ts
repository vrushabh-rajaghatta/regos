export interface AttachableProductDocument {
  productDocumentId: string;
  name: string;
  documentType: string;
  currentVersionNumber: number | null;
  status: string;
  createdOnUtc: string;
}
