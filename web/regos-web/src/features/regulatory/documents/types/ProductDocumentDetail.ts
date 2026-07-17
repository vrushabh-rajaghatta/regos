export interface DocumentVersionDetail {
  versionNumber: number;
  originalFileName: string;
  contentType: string;
  fileSize: number;
  uploadedOnUtc: string;
}

export interface ProductDocumentDetail {
  id: string;
  name: string;
  documentTypeName: string;
  status: string;
  productId: string;
  productName: string;
  createdOnUtc: string;
  currentVersion: DocumentVersionDetail | null;
}
