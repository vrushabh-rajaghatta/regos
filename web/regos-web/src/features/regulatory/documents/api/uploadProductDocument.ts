import { apiFetch, buildUrl } from "@/shared/api/apiClient";

export interface UploadProductDocumentRequest {
  documentTypeId: string;
  name: string;
  file: File;
}

export interface UploadProductDocumentResponse {
  id: string;
}

export async function uploadProductDocument(
  globalProductId: string,
  request: UploadProductDocumentRequest
): Promise<UploadProductDocumentResponse> {
  const form = new FormData();
  form.append("documentTypeId", request.documentTypeId);
  form.append("name", request.name);
  form.append("file", request.file);

  // No Content-Type header — the browser sets the multipart boundary.
  const response = await apiFetch(
    buildUrl(`/api/products/${globalProductId}/documents`),
    {
      method: "POST",
      body: form,
    }
  );

  if (!response.ok) {
    throw new Error(await extractErrorMessage(response));
  }

  return response.json();
}

async function extractErrorMessage(response: Response): Promise<string> {
  try {
    const problem = await response.json();

    if (problem && typeof problem.detail === "string") {
      return problem.detail;
    }
  } catch {
    // Body was not JSON; fall through to the generic message.
  }

  return "Failed to upload Document.";
}
